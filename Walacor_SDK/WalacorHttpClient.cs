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
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Walacor_SDK.Abstractions;
using Walacor_SDK.Exceptions;
using Walacor_SDK.Pipeline;
using Walacor_SDK.Transport;

namespace Walacor_SDK
{
    internal class WalacorHttpClient : IWalacorHttpClient, IDisposable
    {
        private readonly HttpClient _http;
        private readonly IJsonSerializer _json;
        private readonly WalacorHttpClientOptions _opts;
#pragma warning disable CS0414 // Field is assigned but its value is never used
        private readonly bool _ownsHttp;
#pragma warning restore CS0414 // Field is assigned but its value is never used

        public WalacorHttpClient(
            Uri baseAddress,
            IAuthTokenProvider tokens,
            WalacorHttpClientOptions? options = null)
        {
            var factory = new HttpPipelineFactory();
            this._http = factory.Create(baseAddress, tokens, maxRetries: options?.MaxRetries ?? 2);
            this._http.Timeout = options?.Timeout ?? TimeSpan.FromSeconds(100);
            this._json = factory.CreateJsonSerializer();
            this._opts = options ?? new WalacorHttpClientOptions();
            this._ownsHttp = true;
        }

        public WalacorHttpClient(
            HttpClient http,
            IJsonSerializer json,
            WalacorHttpClientOptions? options = null)
        {
            this._http = http ?? throw new ArgumentNullException(nameof(http));
            this._json = json ?? throw new ArgumentNullException(nameof(json));
            this._opts = options ?? new WalacorHttpClientOptions();
        }

        public Uri BaseAddress
        {
            get => this._http.BaseAddress!;
            set => this._http.BaseAddress = value;
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct = default)
        {
            try
            {
                var response = await this._http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct)
                    .ConfigureAwait(false);

                await this.MapErrorsOrReturn(response).ConfigureAwait(false);

                return response;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (HttpRequestException hre)
            {
                var corr = ExtractCorrelation(hre);
                throw new WalacorNetworkException("Network failure while sending HTTP request.", corr, hre);
            }
        }

        public async Task<T> GetJsonAsync<T>(string path, IDictionary<string, string>? query = null, CancellationToken ct = default)
        {
            var uri = AppendQuery(path, query);
            using var req = new HttpRequestMessage(HttpMethod.Get, uri);
            using var res = await this.SendAsync(req, ct).ConfigureAwait(false);
            var json = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
            return this._json.Deserialize<T>(json)!;
        }

        public async Task<TResponse> PostJsonAsync<TRequest, TResponse>(string path, TRequest body, CancellationToken ct = default)
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, path);
            req.Content = new StringContent(this._json.Serialize(body!), Encoding.UTF8, "application/json");
            using var res = await this.SendAsync(req, ct).ConfigureAwait(false);
            var json = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
            return this._json.Deserialize<TResponse>(json)!;
        }

        public async Task<TResponse> PutJsonAsyncTask<TRequest, TResponse>(string path, TRequest body, CancellationToken ct = default)
        {
            using var req = new HttpRequestMessage(HttpMethod.Put, path);
            req.Content = new StringContent(this._json.Serialize(body!), Encoding.UTF8, "application/json");
            using var res = await this.SendAsync(req, ct).ConfigureAwait(false);
            var json = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
            return this._json.Deserialize<TResponse>(json)!;
        }

        public async Task DeleteAsync(string path, CancellationToken ct = default)
        {
            using var req = new HttpRequestMessage(HttpMethod.Delete, path);
            using var res = await this.SendAsync(req, ct).ConfigureAwait(false);
        }

        private async Task MapErrorsOrReturn(HttpResponseMessage res)
        {
            if ((int)res.StatusCode == 422 && this._opts.ThrowOnValidation422)
            {
                var body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                var corr = ExtractCorrelation(res);
                throw new WalacorValidationException("Validation failed (422).", body, corr);
            }

            if ((int)res.StatusCode < 400)
            {
                return;
            }

            var correlation = ExtractCorrelation(res);
            var bodyText = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (res.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new WalacorAuthException("Unauthorized after token refresh.", correlation);
            }

            if ((int)res.StatusCode >= 500)
            {
                throw new WalacorServerException("Server error.", res.StatusCode, bodyText, correlation);
            }

            throw new WalacorRequestException("Request failed.", res.StatusCode, bodyText, correlation);
        }

#pragma warning disable SA1204
        private static string? ExtractCorrelation(HttpResponseMessage res)
#pragma warning restore SA1204
        {
            if (res.RequestMessage?.Properties != null &&
                res.RequestMessage.Properties.TryGetValue(CorrelationLoggingHandler.CorrelationKey, out var v))
            {
                return v as string;
            }

            if (res.Headers.TryGetValues(CorrelationLoggingHandler.CorrelationHeader, out var vals))
            {
                foreach (var x in vals)
                {
                    return x;
                }
            }

            return null;
        }

        private static string? ExtractCorrelation(Exception ex)
        {
            if (ex.Data.Contains(CorrelationLoggingHandler.CorrelationKey))
            {
                return ex.Data[CorrelationLoggingHandler.CorrelationKey]?.ToString();
            }

            return null;
        }

        private static string AppendQuery(string path, IDictionary<string, string>? query)
        {
            if (query == null || query.Count == 0)
            {
                return path;
            }

            var sb = new StringBuilder(path);
            sb.Append(path.Contains("?") ? "&" : "?");
            bool first = true;
            foreach (var kv in query)
            {
                if (!first)
                {
                    sb.Append("&");
                }

                sb.Append(Uri.EscapeDataString(kv.Key));
                sb.Append("=");
                sb.Append(Uri.EscapeDataString(kv.Value));
                first = false;
            }

            return sb.ToString();
        }

#pragma warning disable SA1202
        public void Dispose()
#pragma warning restore SA1202
        {
            if (this._ownsHttp)
            {
                this._http.Dispose();
            }
        }
    }
}
