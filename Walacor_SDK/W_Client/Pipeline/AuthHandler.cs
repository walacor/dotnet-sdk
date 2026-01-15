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
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Walacor_SDK.Client.Extensions;
using Walacor_SDK.W_Client.Abstractions;

namespace Walacor_SDK.Client.Pipeline
{
    internal sealed class AuthHandler : DelegatingHandler
    {
        private const string BearerScheme = "Bearer";
        private const string BearerPrefix = "Bearer ";

        private readonly IAuthTokenProvider _tokens;
        private readonly SemaphoreSlim _refreshLock = new SemaphoreSlim(1, 1);

        public AuthHandler(IAuthTokenProvider tokens, HttpMessageHandler inner)
            : base(inner)
        {
            this._tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
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

            await this._refreshLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                token = await this._tokens.RefreshTokenAsync(ct).ConfigureAwait(false);
            }
            finally
            {
                this._refreshLock.Release();
            }

            var retry = await request.CloneAsync().ConfigureAwait(false);
            TryApplyBearer(retry, token);

            var second = await base.SendAsync(retry, ct).ConfigureAwait(false);
            return second;
        }

        private static void TryApplyBearer(HttpRequestMessage request, string? token)
        {
            var normalized = NormalizeBearerToken(token);
            if (string.IsNullOrEmpty(normalized))
            {
                // If token is invalid/empty, don't set Authorization at all.
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

            var t = token!.Trim();

            // Some providers return "Bearer <token>" already.
            if (t.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
            {
                t = t.Substring(BearerPrefix.Length).Trim();
            }

            return string.IsNullOrEmpty(t) ? null : t;
        }
    }
}
