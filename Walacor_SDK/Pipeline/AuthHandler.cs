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
using Walacor_SDK.Abstractions;
using Walacor_SDK.Extensions;

namespace Walacor_SDK.Pipeline
{
    internal sealed class AuthHandler(IAuthTokenProvider tokens, HttpMessageHandler inner)
        : DelegatingHandler(inner)
    {
        private readonly IAuthTokenProvider _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
        private readonly SemaphoreSlim _refreshLock = new SemaphoreSlim(1, 1);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var token = await this._tokens.GetTokenAsync(ct).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await base.SendAsync(request, ct).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.Unauthorized)
            {
                return response;
            }

            // 401 Unauthorized - try to refresh token and retry once
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
            if (!string.IsNullOrEmpty(token))
            {
                retry.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var second = await base.SendAsync(retry, ct).ConfigureAwait(false);
            return second;
        }
    }
}
