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
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Walacor_SDK.Abstractions;
using Walacor_SDK.Models;

namespace Walacor_SDK.Auth
{
    public sealed class UsernamePasswordTokenProvider : IAuthTokenProvider, IDisposable
    {
        private readonly HttpClient _authClient;
        private readonly string _userName;
        private readonly string _password;

        // In-memory token cache
        private string _token = string.Empty;

        public UsernamePasswordTokenProvider(Uri baseAddress, string userName, string password)
        {
            if (baseAddress == null)
            {
                throw new ArgumentNullException(nameof(baseAddress));
            }

            this._userName = userName ?? throw new ArgumentNullException(nameof(userName));
            this._password = password ?? throw new ArgumentNullException(nameof(password));

            // Separate client for auth (no auth handler on itself)
            this._authClient = new HttpClient(new HttpClientHandler())
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(30),
            };
        }

        public Task<string> GetTokenAsync(CancellationToken ct)
        {
            // Return cached token if present; otherwise force a refresh.
            if (!string.IsNullOrEmpty(this._token))
            {
                return Task.FromResult(this._token);
            }

            return this.RefreshTokenAsync(ct);
        }

        public async Task<string> RefreshTokenAsync(CancellationToken ct)
        {
            var bodyObj = new { userName = this._userName, password = this._password };
            var content = new StringContent(JsonConvert.SerializeObject(bodyObj), Encoding.UTF8, "application/json");

            using var res = await this._authClient.PostAsync("/auth/login", content, ct).ConfigureAwait(false);
            res.EnsureSuccessStatusCode();

            var json = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

            var dto = JsonConvert.DeserializeObject<LoginDto>(json);
            if (dto == null || string.IsNullOrEmpty(dto.ApiToken))
            {
                throw new InvalidOperationException("Authentication succeeded but no token was returned.");
            }

            this._token = dto.ApiToken; // cache it in memory
            return this._token;
        }

        public void Dispose() => this._authClient.Dispose();
    }
}
