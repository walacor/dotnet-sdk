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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Walacor_SDK.W_Client.Constants;
using Walacor_SDK.W_Client.Options;

namespace Walacor_SDK.Client.Pipeline
{
    internal sealed class SdkLoggingHandler : DelegatingHandler
    {
        private readonly ILogger _logger;
        private readonly WalacorHttpClientOptions _options;

        public SdkLoggingHandler(
            ILogger logger,
            WalacorHttpClientOptions options,
            HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            this._logger = logger ?? NullLogger.Instance;
            this._options = options ?? throw new ArgumentNullException(nameof(options));
        }

#pragma warning disable MA0051 // Method is too long
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
#pragma warning restore MA0051 // Method is too long
        {
            var correlationId = TryGetCorrelationId(request);
            var path = request.RequestUri?.PathAndQuery ?? string.Empty;
            var stopwatch = Stopwatch.StartNew();

            this._logger.LogDebug(
                SdkHttpLoggingConstants.HttpRequestStart,
                SdkHttpLoggingConstants.MsgRequestStarted,
                request.Method,
                path,
                correlationId ?? string.Empty);

            if (this._options.LogRequestHeaders)
            {
                var headerText = FormatHeaders(request.Headers, request.Content?.Headers, this._options.RedactHeader);
                this._logger.LogDebug(
                    SdkHttpLoggingConstants.HttpRequestHeaders,
                    SdkHttpLoggingConstants.MsgRequestHeaders,
                    request.Method,
                    path,
                    correlationId ?? string.Empty,
                    headerText);
            }

            if (this._options.LogBodies && ShouldLogBody(request.RequestUri, request.Content?.Headers?.ContentType))
            {
                var body = await TryReadContentAsync(request.Content).ConfigureAwait(false);
                if (body != null)
                {
                    this._logger.LogDebug(
                        SdkHttpLoggingConstants.HttpRequestBody,
                        SdkHttpLoggingConstants.MsgRequestBody,
                        request.Method,
                        path,
                        correlationId ?? string.Empty,
                        body);
                }
            }

            try
            {
                var response = await base.SendAsync(request, ct).ConfigureAwait(false);
                stopwatch.Stop();

                var statusCode = (int)response.StatusCode;
                var level = response.IsSuccessStatusCode ? this._options.SuccessLevel : this._options.FailureLevel;

                this._logger.Log(
                    level,
                    SdkHttpLoggingConstants.HttpRequestStop,
                    SdkHttpLoggingConstants.MsgRequestCompleted,
                    request.Method,
                    path,
                    statusCode,
                    stopwatch.ElapsedMilliseconds,
                    correlationId ?? string.Empty);

                if (this._options.LogResponseHeaders)
                {
                    var headerText = FormatHeaders(response.Headers, response.Content?.Headers, this._options.RedactHeader);
                    this._logger.LogDebug(
                        SdkHttpLoggingConstants.HttpResponseHeaders,
                        SdkHttpLoggingConstants.MsgResponseHeaders,
                        request.Method,
                        path,
                        correlationId ?? string.Empty,
                        headerText);
                }

                if (this._options.LogBodies && ShouldLogBody(request.RequestUri, response.Content?.Headers?.ContentType))
                {
                    var body = await TryReadResponseContentAsync(response.Content).ConfigureAwait(false);
                    if (body != null)
                    {
                        this._logger.LogDebug(
                            SdkHttpLoggingConstants.HttpResponseBody,
                            SdkHttpLoggingConstants.MsgResponseBody,
                            request.Method,
                            path,
                            correlationId ?? string.Empty,
                            body);
                    }
                }

                return response;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                stopwatch.Stop();
                this._logger.Log(
                    this._options.ExceptionLevel,
                    SdkHttpLoggingConstants.HttpRequestException,
                    ex,
                    SdkHttpLoggingConstants.MsgRequestFailed,
                    request.Method,
                    path,
                    stopwatch.ElapsedMilliseconds,
                    correlationId ?? string.Empty);
                throw;
            }
        }

        private static string? TryGetCorrelationId(HttpRequestMessage request)
        {
            if (request.Properties.TryGetValue(SdkHttpLoggingConstants.CorrelationPropertyKey, out var corrObj))
            {
                return corrObj?.ToString();
            }

            if (request.Headers.TryGetValues(SdkHttpLoggingConstants.CorrelationHeader, out var values))
            {
                return values.FirstOrDefault();
            }

            return null;
        }

        private static bool ShouldLogBody(Uri? requestUri, MediaTypeHeaderValue? contentType)
        {
            if (contentType?.MediaType != null &&
                contentType.MediaType.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (contentType?.MediaType != null &&
                contentType.MediaType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (requestUri?.AbsolutePath != null &&
                requestUri.AbsolutePath.IndexOf("/auth", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return false;
            }

            return true;
        }

        private static async Task<string?> TryReadContentAsync(HttpContent? content)
        {
            if (content == null)
            {
                return null;
            }

            try
            {
                await content.LoadIntoBufferAsync().ConfigureAwait(false);
                var body = await content.ReadAsStringAsync().ConfigureAwait(false);
                return string.IsNullOrWhiteSpace(body) ? null : $"{Environment.NewLine}{body}";
            }
            catch
            {
                return null;
            }
        }

        private static async Task<string?> TryReadResponseContentAsync(HttpContent? content)
        {
            if (content == null)
            {
                return null;
            }

            try
            {
                await content.LoadIntoBufferAsync().ConfigureAwait(false);
                var body = await content.ReadAsStringAsync().ConfigureAwait(false);
                return string.IsNullOrWhiteSpace(body) ? null : $"{Environment.NewLine}{body}";
            }
            catch
            {
                return null;
            }
        }

        private static string FormatHeaders(
            HttpHeaders headers,
            HttpHeaders? contentHeaders,
            Func<string, bool> redactHeader)
        {
            var allHeaders = new List<KeyValuePair<string, IEnumerable<string>>>();
            allHeaders.AddRange(headers.Select(h => new KeyValuePair<string, IEnumerable<string>>(h.Key, h.Value)));

            if (contentHeaders != null)
            {
                allHeaders.AddRange(contentHeaders.Select(h => new KeyValuePair<string, IEnumerable<string>>(h.Key, h.Value)));
            }

            if (allHeaders.Count == 0)
            {
                return " (none)";
            }

            var sb = new StringBuilder();
            foreach (var header in allHeaders)
            {
                sb.AppendLine();
                sb.Append(header.Key);
                sb.Append(": ");
                if (redactHeader(header.Key))
                {
                    sb.Append("[REDACTED]");
                }
                else
                {
                    sb.Append(string.Join(", ", header.Value));
                }
            }

            return sb.ToString();
        }
    }
}
