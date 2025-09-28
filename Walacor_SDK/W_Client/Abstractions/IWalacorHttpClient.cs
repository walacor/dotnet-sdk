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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Walacor_SDK.W_Client.Abstractions
{
    internal interface IWalacorHttpClient
    {
        Uri BaseAddress { get; set; }

        Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken ct = default);

        Task<T> GetJsonAsync<T>(
            string path,
            IDictionary<string, string>? query = null,
            CancellationToken ct = default);

        Task<TResponse> PostJsonAsync<TRequest, TResponse>(
            string path,
            TRequest body,
            CancellationToken ct = default);

        Task<TResponse> PutJsonAsync<TRequest, TResponse>(
            string path,
            TRequest body,
            CancellationToken ct = default);

        Task DeleteAsync(
            string path,
            CancellationToken ct = default);

        Task<T> GetJsonAsync<T>(
            string path,
            IDictionary<string, string>? query = null,
            IDictionary<string, string>? headers = null,
            CancellationToken ct = default);

        Task<TResponse> PostJsonAsync<TRequest, TResponse>(
            string path,
            TRequest body,
            IDictionary<string, string>? query = null,
            IDictionary<string, string>? headers = null,
            CancellationToken ct = default);

        Task<TResponse> PutJsonAsync<TRequest, TResponse>(
            string path,
            TRequest body,
            IDictionary<string, string>? query = null,
            IDictionary<string, string>? headers = null,
            CancellationToken ct = default);

        Task DeleteAsync(
            string path,
            IDictionary<string, string>? query = null,
            IDictionary<string, string>? headers = null,
            CancellationToken ct = default);

        // Files TODO
        // Task UploadAsync(string path, Stream fileStream, string formFieldName = "file", IDictionary<string, string>? additionalFields = null, CancellationToken ct = default);
        // Task DownloadStreamAsync(string path, Stream destination, CancellationToken ct = default);
    }
}
