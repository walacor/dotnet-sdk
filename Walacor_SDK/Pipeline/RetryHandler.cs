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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Walacor_SDK.Abstractions;
using Walacor_SDK.Extensions;

namespace Walacor_SDK.Pipeline
{
    internal sealed class RetryHandler : DelegatingHandler
    {
        private static readonly HttpStatusCode[] TransientCodes =
        [
            HttpStatusCode.RequestTimeout, // 408
            (HttpStatusCode)429, // Too Many Requests
            HttpStatusCode.InternalServerError, // 500
            HttpStatusCode.BadGateway, // 502
            HttpStatusCode.ServiceUnavailable, // 503
            HttpStatusCode.GatewayTimeout, // 504
        ];

        private readonly IBackoffStrategy _backoff;
        private readonly int _maxRetries;

        public RetryHandler(
            IBackoffStrategy backoff,
            int maxRetries,
            HttpMessageHandler inner)
            : base(inner)
        {
            this._backoff = backoff ?? throw new ArgumentNullException(nameof(backoff));
            this._maxRetries = Math.Max(0, maxRetries);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var isIdempotentRead = request.Method == HttpMethod.Get || request.Method == HttpMethod.Head;
            if (!isIdempotentRead)
            {
                return await base.SendAsync(request, ct).ConfigureAwait(false);
            }

            int attempt = 0;
            while (true)
            {
                attempt++;

                try
                {
                    var response = await base
                        .SendAsync(attempt == 1 ? request : await request.CloneAsync().ConfigureAwait(false), ct)
                        .ConfigureAwait(false);

                    if (!TransientCodes.Contains(response.StatusCode) || attempt > this._maxRetries + 1)
                    {
                        return response;
                    }

                    var retryAfterDelay = GetRetryAfterDelay(response);
                    var delay = retryAfterDelay ?? this._backoff.ComputeDelay(attempt);
                    response.Dispose();
                    await Task.Delay(delay, ct).ConfigureAwait(false);
                    continue;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (HttpRequestException)
                {
                    if (attempt > this._maxRetries + 1)
                    {
                        throw;
                    }

                    var delay = this._backoff.ComputeDelay(attempt);
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
    }
}
