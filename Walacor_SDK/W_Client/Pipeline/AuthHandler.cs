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
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Walacor_SDK.Client.Extensions;
using Walacor_SDK.W_Client.Abstractions;
using Walacor_SDK.W_Client.Constants;

namespace Walacor_SDK.Client.Pipeline
{
    internal sealed class AuthHandler : DelegatingHandler
    {
        private const string BearerScheme = "Bearer";
        private const string BearerPrefix = "Bearer ";

        private readonly IAuthTokenProvider _tokens;
        private readonly SemaphoreSlim _refreshLock = new SemaphoreSlim(1, 1);
        private readonly ILogger _logger;

        public AuthHandler(IAuthTokenProvider tokens, ILogger logger, HttpMessageHandler inner)
            : base(inner)
        {
            this._tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
            this._logger = logger ?? NullLogger.Instance;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var token = await this._tokens.GetTokenAsync(ct).ConfigureAwait(false);
            TryApplyBearer(request, token);

            var response = await base.SendAsync(request, ct).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.Unauthorized)
            {
                return response;
            }

            response.Dispose();

            this._logger.LogDebug(
                AuthLoggingConstants.RefreshingToken,
                AuthLoggingConstants.MsgRefreshingToken,
                GetCorrelationId(request) ?? string.Empty);

            await this._refreshLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                token = await this._tokens.RefreshTokenAsync(ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this._logger.LogWarning(
                    AuthLoggingConstants.TokenRefreshFailed,
                    ex,
                    AuthLoggingConstants.MsgTokenRefreshFailed,
                    GetCorrelationId(request) ?? string.Empty);
                throw;
            }
            finally
            {
                this._refreshLock.Release();
            }

            var retry = await request.CloneAsync().ConfigureAwait(false);
            TryApplyBearer(retry, token);

            this._logger.LogDebug(
                AuthLoggingConstants.RetryingAfterRefresh,
                AuthLoggingConstants.MsgRetryingAfterRefresh,
                GetCorrelationId(request) ?? string.Empty);

            return await base.SendAsync(retry, ct).ConfigureAwait(false);
        }

        private static void TryApplyBearer(HttpRequestMessage request, string? token)
        {
            var normalized = NormalizeBearerToken(token);
            if (string.IsNullOrEmpty(normalized))
            {
                request.Headers.Authorization = null;
                return;
            }

            request.Headers.Authorization = new AuthenticationHeaderValue(BearerScheme, normalized);
        }

        private static string? NormalizeBearerToken(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            var t = token?.Trim();

            if (t!.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
            {
                t = t.Substring(BearerPrefix.Length).Trim();
            }

            return string.IsNullOrEmpty(t) ? null : t;
        }

        private static string? GetCorrelationId(HttpRequestMessage request)
        {
            if (request.Properties.TryGetValue(AuthLoggingConstants.CorrelationPropertyKey, out var corrObj))
            {
                return corrObj?.ToString();
            }

            if (request.Headers.TryGetValues(AuthLoggingConstants.CorrelationHeader, out var values))
            {
                return values.FirstOrDefault();
            }

            return null;
        }
    }
}
