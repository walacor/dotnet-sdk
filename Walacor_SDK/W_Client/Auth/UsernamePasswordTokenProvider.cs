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
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Walacor_SDK.Client.Exceptions;
using Walacor_SDK.Models.Auth;
using Walacor_SDK.W_Client.Abstractions;
using Walacor_SDK.W_Client.Constants;
using Walacor_SDK.W_Client.Mappers;
using Walacor_SDK.W_Client.Options;

namespace Walacor_SDK.W_Client.Auth
{
    internal sealed class UsernamePasswordTokenProvider : IAuthTokenProvider, IDisposable
    {
        private readonly HttpClient _authClient;
        private readonly string _userName;
        private readonly string _password;

        private string _token = string.Empty;

        public UsernamePasswordTokenProvider(Uri baseAddress, string userName, string password, WalacorHttpClientOptions? options = null)
        {
            if (baseAddress == null)
            {
                throw new ArgumentNullException(nameof(baseAddress));
            }

            this._userName = userName ?? throw new ArgumentNullException(nameof(userName));
            this._password = password ?? throw new ArgumentNullException(nameof(password));
            var opts = options ?? new WalacorHttpClientOptions();

            this._authClient = new HttpClient(new HttpClientHandler())
            {
                BaseAddress = baseAddress,
                Timeout = opts.Timeout,
            };
        }

        public async Task<string> GetTokenAsync(CancellationToken ct = default)
        {
            if (!string.IsNullOrEmpty(this._token))
            {
                return this._token;
            }

            return await this.RefreshTokenAsync(ct).ConfigureAwait(false);
        }

        public async Task<string> RefreshTokenAsync(CancellationToken ct = default)
        {
            var bodyObj = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [JsonFieldNames.UserName] = this._userName,
                [JsonFieldNames.Password] = this._password,
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(bodyObj),
                Encoding.UTF8,
                MediaTypeNames.ApplicationJson);

            using var res = await this._authClient.PostAsync(AuthRoutes.Login, content, ct).ConfigureAwait(false);
            var body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!res.IsSuccessStatusCode)
            {
                var status = (int)res.StatusCode;
                var err = HttpErrorMapper.FromStatus(status, body);
                throw new WalacorRequestException(err.Message, res.StatusCode, body);
            }

            var dto = JsonConvert.DeserializeObject<LoginDto>(body);
            if (dto == null || string.IsNullOrEmpty(dto.ApiToken))
            {
                throw new InvalidOperationException(ErrorMessages.AuthSucceededNoToken);
            }

            this._token = dto.ApiToken.StartsWith(MediaTypeNames.BearerPrefix, StringComparison.OrdinalIgnoreCase)
                ? dto.ApiToken.Substring(MediaTypeNames.BearerPrefix.Length)
                : dto.ApiToken;

            return this._token;
        }

        public void Dispose() => this._authClient.Dispose();
    }
}
