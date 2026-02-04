// Copyright 2025 Walacor Corporation
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Walacor_SDK.Client.Extensions;
using Walacor_SDK.W_Client.Abstractions;
using Walacor_SDK.W_Client.Constants;

namespace Walacor_SDK.Client.Pipeline
{
    internal sealed class RetryHandler : DelegatingHandler
    {
        private static readonly HttpStatusCode[] TransientCodes =
        [
            HttpStatusCode.RequestTimeout,       // 408
            (HttpStatusCode)429,                 // Too Many Requests
            HttpStatusCode.InternalServerError,  // 500
            HttpStatusCode.BadGateway,           // 502
            HttpStatusCode.ServiceUnavailable,   // 503
            HttpStatusCode.GatewayTimeout,       // 504
        ];

        private readonly IBackoffStrategy _backoff;
        private readonly int _maxRetries;
        private readonly ILogger _logger;

        public RetryHandler(
            IBackoffStrategy backoff,
            int maxRetries,
            ILogger logger,
            HttpMessageHandler inner)
            : base(inner)
        {
            this._backoff = backoff ?? throw new ArgumentNullException(nameof(backoff));
            this._maxRetries = Math.Max(0, maxRetries);
            this._logger = logger ?? NullLogger.Instance;
        }

#pragma warning disable MA0051 // Method is too long
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
#pragma warning restore MA0051 // Method is too long
        {
            // Retrying non-idempotent operations can cause duplicate side effects.
            var isIdempotentRead = request.Method == HttpMethod.Get || request.Method == HttpMethod.Head;
            if (!isIdempotentRead)
            {
                return await base.SendAsync(request, ct).ConfigureAwait(false);
            }

            var correlationId = GetCorrelationId(request) ?? string.Empty;
            var method = request.Method.Method;
            var path = request.RequestUri?.PathAndQuery ?? string.Empty;

            // maxRetries=0 => maxAttempts=1 (no retries).
            var maxAttempts = this._maxRetries + 1;

            var totalSw = Stopwatch.StartNew();
            var attempt = 0;

            while (true)
            {
                attempt++;

                // Each attempt duration (useful for diagnosing slowness vs backoff)
                var attemptSw = Stopwatch.StartNew();

                try
                {
                    var toSend = attempt == 1 ? request : await request.CloneAsync().ConfigureAwait(false);
                    var response = await base.SendAsync(toSend, ct).ConfigureAwait(false);

                    attemptSw.Stop();

                    var statusCode = (int)response.StatusCode;
                    var isTransient = TransientCodes.Contains(response.StatusCode);

                    // Not transient => return immediately (no retry).
                    if (!isTransient)
                    {
                        return response;
                    }

                    // Transient but we've exhausted attempts => log and return the last response.
                    if (attempt >= maxAttempts)
                    {
                        totalSw.Stop();

                        this._logger.LogWarning(
                            RetryLoggingConstants.MaxRetriesReached,
                            RetryLoggingConstants.MsgMaxRetriesReachedWithStatusAndTiming,
                            method,
                            path,
                            attempt,
                            maxAttempts,
                            statusCode,
                            attemptSw.ElapsedMilliseconds,
                            totalSw.ElapsedMilliseconds,
                            correlationId);

                        return response;
                    }

                    // Compute delay: Retry-After (if present) wins, otherwise exponential backoff strategy.
                    var retryAfterDelay = GetRetryAfterDelay(response);
                    var delay = retryAfterDelay ?? this._backoff.ComputeDelay(attempt);

                    this._logger.LogWarning(
                        RetryLoggingConstants.RetryingRequest,
                        RetryLoggingConstants.MsgRetryingWithStatusAndTiming,
                        method,
                        path,
                        attempt,
                        maxAttempts,
                        delay.TotalMilliseconds,
                        statusCode,
                        attemptSw.ElapsedMilliseconds,
                        correlationId);

                    response.Dispose();
                    await Task.Delay(delay, ct).ConfigureAwait(false);
                    continue;
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    attemptSw.Stop();
                    totalSw.Stop();

                    this._logger.LogDebug(
                        RetryLoggingConstants.RequestCancelled,
                        RetryLoggingConstants.MsgRequestCancelled,
                        method,
                        path,
                        attempt,
                        attemptSw.ElapsedMilliseconds,
                        totalSw.ElapsedMilliseconds,
                        correlationId);

                    throw;
                }
                catch (HttpRequestException ex)
                {
                    attemptSw.Stop();

                    if (attempt >= maxAttempts)
                    {
                        totalSw.Stop();

                        this._logger.LogWarning(
                            RetryLoggingConstants.MaxRetriesReached,
                            ex,
                            RetryLoggingConstants.MsgMaxRetriesReachedNetworkFailureWithTiming,
                            method,
                            path,
                            attempt,
                            maxAttempts,
                            attemptSw.ElapsedMilliseconds,
                            totalSw.ElapsedMilliseconds,
                            correlationId);

                        throw;
                    }

                    var delay = this._backoff.ComputeDelay(attempt);

                    this._logger.LogWarning(
                        RetryLoggingConstants.RetryingRequest,
                        ex,
                        RetryLoggingConstants.MsgRetryingNetworkFailureWithTiming,
                        method,
                        path,
                        attempt,
                        maxAttempts,
                        delay.TotalMilliseconds,
                        attemptSw.ElapsedMilliseconds,
                        correlationId);

                    await Task.Delay(delay, ct).ConfigureAwait(false);
                    continue;
                }
            }
        }

        private static TimeSpan? GetRetryAfterDelay(HttpResponseMessage response)
        {
            if (response.Headers.RetryAfter == null)
            {
                return null;
            }

            if (response.Headers.RetryAfter.Delta != null)
            {
                return response.Headers.RetryAfter.Delta;
            }

            if (response.Headers.RetryAfter.Date != null)
            {
                var delta = response.Headers.RetryAfter.Date.Value - DateTimeOffset.UtcNow;
                return delta > TimeSpan.Zero ? delta : TimeSpan.Zero;
            }

            return null;
        }

        private static string? GetCorrelationId(HttpRequestMessage request)
        {
            if (request.Properties.TryGetValue(RetryLoggingConstants.CorrelationPropertyKey, out var corrObj))
            {
                return corrObj?.ToString();
            }

            if (request.Headers.TryGetValues(RetryLoggingConstants.CorrelationHeader, out var values))
            {
                return values.FirstOrDefault();
            }

            return null;
        }
    }
}
