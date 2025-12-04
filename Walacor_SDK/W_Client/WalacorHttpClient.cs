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
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Walacor_SDK.Client.Transport;
using Walacor_SDK.Models.Result;
using Walacor_SDK.Models.Results;
using Walacor_SDK.W_Client.Abstractions;
using Walacor_SDK.W_Client.Mappers;
using Walacor_SDK.W_Client.Options;

namespace Walacor_SDK.Client
{
    internal class WalacorHttpClient : IWalacorHttpClient, IDisposable
    {
        private readonly HttpClient _http;
        private readonly IJsonSerializer _json;
        private readonly WalacorHttpClientOptions _opts;
        private readonly bool _ownsHttp;

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
            return await this._http.SendAsync(request, HttpCompletionOption.ResponseContentRead, ct)
                              .ConfigureAwait(false);
        }

        public async Task<Result<TResponse>> GetJsonAsync<TResponse>(
            string path,
            IDictionary<string, string>? query = null,
            CancellationToken ct = default)
        {
            try
            {
                var uri = AppendQuery(path, query);
                using var req = new HttpRequestMessage(HttpMethod.Get, uri);
                req.Headers.Accept.ParseAdd("application/json");

                using var res = await this.SendAsync(req, ct).ConfigureAwait(false);

                var (corrId, duration) = TryGetCorrelationInfo(res);
                var status = (int)res.StatusCode;

                if (status == 204)
                {
                    return Result<TResponse>.Fail(Error.NotFound("No content."), status, corrId, duration);
                }

                var mediaType = res.Content.Headers.ContentType?.MediaType;
                var body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (res.IsSuccessStatusCode)
                {
                    if (string.IsNullOrWhiteSpace(body))
                    {
                        return Result<TResponse>.Fail(Error.Deserialization("Empty response body."), status, corrId, duration);
                    }

                    if (!string.Equals(mediaType, "application/json", StringComparison.OrdinalIgnoreCase))
                    {
                        return Result<TResponse>.Fail(Error.Deserialization($"Unexpected content type: {mediaType ?? "unknown"}"), status, corrId, duration);
                    }

                    return ResponseMapper.FromSuccessEnvelope<TResponse>(
                        body,
                        s => this._json.Deserialize<BaseResponse<TResponse>>(s),
                        status,
                        corrId,
                        duration);
                }

                var err = HttpErrorMapper.FromStatus(status, body);
                return Result<TResponse>.Fail(err, status, corrId, duration);
            }
            catch (Exception ex)
            {
                return ExceptionResult.From<TResponse>(ex);
            }
        }

        public async Task<Result<TResponse>> PostJsonAsync<TRequest, TResponse>(
            string path,
            TRequest body,
            CancellationToken ct = default)
        {
            try
            {
                var uri = AppendQuery(path, query: null);
                using var req = new HttpRequestMessage(HttpMethod.Post, uri);
                req.Headers.Accept.ParseAdd("application/json");
                req.Content = new StringContent(this._json.Serialize(body!), Encoding.UTF8, "application/json");

                using var res = await this.SendAsync(req, ct).ConfigureAwait(false);

                var (corrId, duration) = TryGetCorrelationInfo(res);
                var status = (int)res.StatusCode;

                if (status == 204)
                {
                    return Result<TResponse>.Fail(Error.NotFound("No content."), status, corrId, duration);
                }

                var mediaType = res.Content.Headers.ContentType?.MediaType;
                var bodyText = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (res.IsSuccessStatusCode)
                {
                    if (string.IsNullOrWhiteSpace(bodyText))
                    {
                        return Result<TResponse>.Fail(Error.Deserialization("Empty response body."), status, corrId, duration);
                    }

                    if (!string.Equals(mediaType, "application/json", StringComparison.OrdinalIgnoreCase))
                    {
                        return Result<TResponse>.Fail(Error.Deserialization($"Unexpected content type: {mediaType ?? "unknown"}"), status, corrId, duration);
                    }

                    return ResponseMapper.FromSuccessEnvelope<TResponse>(
                        bodyText,
                        s => this._json.Deserialize<BaseResponse<TResponse>>(s),
                        status,
                        corrId,
                        duration);
                }

                var err = HttpErrorMapper.FromStatus(status, bodyText);
                return Result<TResponse>.Fail(err, status, corrId, duration);
            }
            catch (Exception ex)
            {
                return ExceptionResult.From<TResponse>(ex);
            }
        }

        public async Task<Result<TResponse>> PutJsonAsync<TRequest, TResponse>(
            string path,
            TRequest body,
            CancellationToken ct = default)
        {
            try
            {
                var uri = AppendQuery(path, query: null);
                using var req = new HttpRequestMessage(HttpMethod.Put, uri);
                req.Headers.Accept.ParseAdd("application/json");
                req.Content = new StringContent(this._json.Serialize(body!), Encoding.UTF8, "application/json");

                using var res = await this.SendAsync(req, ct).ConfigureAwait(false);

                var (corrId, duration) = TryGetCorrelationInfo(res);
                var status = (int)res.StatusCode;

                if (status == 204)
                {
                    return Result<TResponse>.Fail(Error.NotFound("No content."), status, corrId, duration);
                }

                var mediaType = res.Content.Headers.ContentType?.MediaType;
                var bodyText = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (res.IsSuccessStatusCode)
                {
                    if (string.IsNullOrWhiteSpace(bodyText))
                    {
                        return Result<TResponse>.Fail(Error.Deserialization("Empty response body."), status, corrId, duration);
                    }

                    if (!string.Equals(mediaType, "application/json", StringComparison.OrdinalIgnoreCase))
                    {
                        return Result<TResponse>.Fail(Error.Deserialization($"Unexpected content type: {mediaType ?? "unknown"}"), status, corrId, duration);
                    }

                    return ResponseMapper.FromSuccessEnvelope<TResponse>(
                        bodyText,
                        s => this._json.Deserialize<BaseResponse<TResponse>>(s),
                        status,
                        corrId,
                        duration);
                }

                var err = HttpErrorMapper.FromStatus(status, bodyText);
                return Result<TResponse>.Fail(err, status, corrId, duration);
            }
            catch (Exception ex)
            {
                return ExceptionResult.From<TResponse>(ex);
            }
        }

        public async Task<Result<bool>> DeleteAsync(
            string path,
            CancellationToken ct = default)
        {
            try
            {
                var uri = AppendQuery(path, query: null);
                using var req = new HttpRequestMessage(HttpMethod.Delete, uri);
                req.Headers.Accept.ParseAdd("application/json");

                using var res = await this.SendAsync(req, ct).ConfigureAwait(false);

                var (corrId, duration) = TryGetCorrelationInfo(res);
                var status = (int)res.StatusCode;

                if (res.IsSuccessStatusCode || status == 204)
                {
                    return Result<bool>.Success(true, status, corrId, duration);
                }

                var bodyText = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                var err = HttpErrorMapper.FromStatus(status, bodyText);
                return Result<bool>.Fail(err, status, corrId, duration);
            }
            catch (Exception ex)
            {
                return ExceptionResult.From<bool>(ex);
            }
        }

        public async Task<Result<TResponse>> GetJsonWithHeadersAsync<TResponse>(
           string path,
           IDictionary<string, string>? query = null,
           IDictionary<string, string>? headers = null,
           CancellationToken ct = default)
        {
            try
            {
                var uri = AppendQuery(path, query);
                using var req = new HttpRequestMessage(HttpMethod.Get, uri);
                req.Headers.Accept.ParseAdd("application/json");
                ApplyHeaders(req, headers);

                using var res = await this.SendAsync(req, ct).ConfigureAwait(false);

                var (corrId, duration) = TryGetCorrelationInfo(res);
                var status = (int)res.StatusCode;

                if (status == 204)
                {
                    return Result<TResponse>.Fail(Error.NotFound("No content."), status, corrId, duration);
                }

                var mediaType = res.Content.Headers.ContentType?.MediaType;
                var bodyText = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (res.IsSuccessStatusCode)
                {
                    if (string.IsNullOrWhiteSpace(bodyText))
                    {
                        return Result<TResponse>.Fail(Error.Deserialization("Empty response body."), status, corrId, duration);
                    }

                    if (!string.Equals(mediaType, "application/json", StringComparison.OrdinalIgnoreCase))
                    {
                        return Result<TResponse>.Fail(Error.Deserialization($"Unexpected content type: {mediaType ?? "unknown"}"), status, corrId, duration);
                    }

                    return ResponseMapper.FromSuccessEnvelope<TResponse>(
                        bodyText,
                        s => this._json.Deserialize<BaseResponse<TResponse>>(s),
                        status,
                        corrId,
                        duration);
                }

                var err = HttpErrorMapper.FromStatus(status, bodyText);
                return Result<TResponse>.Fail(err, status, corrId, duration);
            }
            catch (Exception ex)
            {
                return ExceptionResult.From<TResponse>(ex);
            }
        }

        public async Task<Result<TResponse>> PostJsonWithHeadersAsync<TRequest, TResponse>(
            string path,
            TRequest body,
            IDictionary<string, string>? query = null,
            IDictionary<string, string>? headers = null,
            CancellationToken ct = default)
        {
            try
            {
                var uri = AppendQuery(path, query);
                using var req = new HttpRequestMessage(HttpMethod.Post, uri);
                req.Headers.Accept.ParseAdd("application/json");
                req.Content = new StringContent(this._json.Serialize(body!), Encoding.UTF8, "application/json");
                ApplyHeaders(req, headers);

                using var res = await this.SendAsync(req, ct).ConfigureAwait(false);

                var (corrId, duration) = TryGetCorrelationInfo(res);
                var status = (int)res.StatusCode;

                if (status == 204)
                {
                    return Result<TResponse>.Fail(Error.NotFound("No content."), status, corrId, duration);
                }

                var mediaType = res.Content.Headers.ContentType?.MediaType;
                var bodyText = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (res.IsSuccessStatusCode)
                {
                    if (string.IsNullOrWhiteSpace(bodyText))
                    {
                        return Result<TResponse>.Fail(Error.Deserialization("Empty response body."), status, corrId, duration);
                    }

                    if (!string.Equals(mediaType, "application/json", StringComparison.OrdinalIgnoreCase))
                    {
                        return Result<TResponse>.Fail(Error.Deserialization($"Unexpected content type: {mediaType ?? "unknown"}"), status, corrId, duration);
                    }

                    return ResponseMapper.FromSuccessEnvelope<TResponse>(
                        bodyText,
                        s => this._json.Deserialize<BaseResponse<TResponse>>(s),
                        status,
                        corrId,
                        duration);
                }

                var err = HttpErrorMapper.FromStatus(status, bodyText);
                return Result<TResponse>.Fail(err, status, corrId, duration);
            }
            catch (Exception ex)
            {
                return ExceptionResult.From<TResponse>(ex);
            }
        }

        public async Task<Result<TResponse>> PutJsonWithHeadersAsync<TRequest, TResponse>(
            string path,
            TRequest body,
            IDictionary<string, string>? query = null,
            IDictionary<string, string>? headers = null,
            CancellationToken ct = default)
        {
            try
            {
                var uri = AppendQuery(path, query);
                using var req = new HttpRequestMessage(HttpMethod.Put, uri);
                req.Headers.Accept.ParseAdd("application/json");
                req.Content = new StringContent(this._json.Serialize(body!), Encoding.UTF8, "application/json");
                ApplyHeaders(req, headers);

                using var res = await this.SendAsync(req, ct).ConfigureAwait(false);

                var (corrId, duration) = TryGetCorrelationInfo(res);
                var status = (int)res.StatusCode;

                if (status == 204)
                {
                    return Result<TResponse>.Fail(Error.NotFound("No content."), status, corrId, duration);
                }

                var mediaType = res.Content.Headers.ContentType?.MediaType;
                var bodyText = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (res.IsSuccessStatusCode)
                {
                    if (string.IsNullOrWhiteSpace(bodyText))
                    {
                        return Result<TResponse>.Fail(Error.Deserialization("Empty response body."), status, corrId, duration);
                    }

                    if (!string.Equals(mediaType, "application/json", StringComparison.OrdinalIgnoreCase))
                    {
                        return Result<TResponse>.Fail(Error.Deserialization($"Unexpected content type: {mediaType ?? "unknown"}"), status, corrId, duration);
                    }

                    return ResponseMapper.FromSuccessEnvelope<TResponse>(
                        bodyText,
                        s => this._json.Deserialize<BaseResponse<TResponse>>(s),
                        status,
                        corrId,
                        duration);
                }

                var err = HttpErrorMapper.FromStatus(status, bodyText);
                return Result<TResponse>.Fail(err, status, corrId, duration);
            }
            catch (TaskCanceledException)
            {
                return Result<TResponse>.Fail(Error.Timeout(), null, null);
            }
            catch (HttpRequestException)
            {
                return Result<TResponse>.Fail(Error.Network(), null, null);
            }
            catch (Exception)
            {
                return Result<TResponse>.Fail(Error.Unknown(), null, null);
            }
        }

        public async Task<Result<bool>> DeleteWithHeadersAsync(
            string path,
            IDictionary<string, string>? query = null,
            IDictionary<string, string>? headers = null,
            CancellationToken ct = default)
        {
            try
            {
                var uri = AppendQuery(path, query);
                using var req = new HttpRequestMessage(HttpMethod.Delete, uri);
                req.Headers.Accept.ParseAdd("application/json");
                ApplyHeaders(req, headers);

                using var res = await this.SendAsync(req, ct).ConfigureAwait(false);

                var (corrId, duration) = TryGetCorrelationInfo(res);
                var status = (int)res.StatusCode;

                if (res.IsSuccessStatusCode || status == 204)
                {
                    return Result<bool>.Success(true, status, corrId, duration);
                }

                var bodyText = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                var err = HttpErrorMapper.FromStatus(status, bodyText);
                return Result<bool>.Fail(err, status, corrId, duration);
            }
            catch (Exception ex)
            {
                return ExceptionResult.From<bool>(ex);
            }
        }

#pragma warning disable MA0051 // Method is too long
        public async Task<Result<TResponse>> PostMultipartAsync<TResponse>(string path, HttpContent content, CancellationToken ct = default)
#pragma warning restore MA0051 // Method is too long
        {
            try
            {
                var uri = AppendQuery(path, query: null);
                using var req = new HttpRequestMessage(HttpMethod.Post, uri);

                req.Headers.Accept.ParseAdd("application/json");
                req.Content = content;

                using var res = await this.SendAsync(req, ct).ConfigureAwait(false);

                var (corrId, duration) = TryGetCorrelationInfo(res);
                var status = (int)res.StatusCode;

                if (status == 204)
                {
                    return Result<TResponse>.Fail(
                        Error.NotFound("No content."),
                        status,
                        corrId,
                        duration);
                }

                var mediaType = res.Content.Headers.ContentType?.MediaType;
                var bodyText = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (res.IsSuccessStatusCode)
                {
                    if (string.IsNullOrWhiteSpace(bodyText))
                    {
                        return Result<TResponse>.Fail(
                            Error.Deserialization("Empty response body."),
                            status,
                            corrId,
                            duration);
                    }

                    if (!string.Equals(mediaType, "application/json", StringComparison.OrdinalIgnoreCase))
                    {
                        return Result<TResponse>.Fail(
                            Error.Deserialization($"Unexpected content type: {mediaType ?? "unknown"}"),
                            status,
                            corrId,
                            duration);
                    }

                    return ResponseMapper.FromSuccessEnvelope<TResponse>(
                        bodyText,
                        s => this._json.Deserialize<BaseResponse<TResponse>>(s),
                        status,
                        corrId,
                        duration);
                }

                var err = HttpErrorMapper.FromStatus(status, bodyText);
                return Result<TResponse>.Fail(err, status, corrId, duration);
            }
            catch (Exception ex)
            {
                return ExceptionResult.From<TResponse>(ex);
            }
        }

        private static void ApplyHeaders(HttpRequestMessage req, IDictionary<string, string>? headers)
        {
            if (headers == null || headers.Count == 0)
            {
                return;
            }

            foreach (var kv in headers)
            {
                var name = kv.Key;
                var value = kv.Value ?? string.Empty;

                if (req.Content != null && name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                {
                    req.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(value);
                    continue;
                }

                req.Headers.Remove(name);
                req.Headers.TryAddWithoutValidation(name, value);
            }
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

        private static (string? CorrelationId, long? DurationMs) TryGetCorrelationInfo(HttpResponseMessage res)
        {
            string? corrId = null;
            long? duration = null;

            if (res.Headers.TryGetValues("X-Request-ID", out var vals))
            {
                corrId = vals.FirstOrDefault();
            }

            if (res.RequestMessage?.Properties.TryGetValue("Walacor.Duration", out var durObj) == true)
            {
                if (durObj is long d)
                {
                    duration = d;
                }
            }

            return (corrId, duration);
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
