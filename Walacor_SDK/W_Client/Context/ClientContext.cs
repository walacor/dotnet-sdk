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
using Walacor_SDK.W_Client.Abstractions;
using Walacor_SDK.W_Client.Helpers;
using Walacor_SDK.W_Client.Options;

namespace Walacor_SDK.W_Client.Context
{
    /// <summary>
    /// Shared context containing the single transport/pipeline and configuration for all services.
    /// </summary>
    internal sealed class ClientContext : IDisposable
    {
        private readonly IAuthTokenProvider _tokenProvider;
        private readonly bool _ownsTransport;
        private readonly bool _ownsTokenProvider;
        private bool _disposed;

        public ClientContext(
            Uri baseUri,
            Uri apiBaseUri,
            IWalacorHttpClient transport,
            WalacorHttpClientOptions options,
            IAuthTokenProvider tokenProvider,
            bool ownsTransport,
            bool ownsTokenProvider)
        {
            this.BaseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));
            this.ApiBaseUri = apiBaseUri ?? throw new ArgumentNullException(nameof(apiBaseUri));
            this.Transport = transport ?? throw new ArgumentNullException(nameof(transport));
            this.Options = options ?? throw new ArgumentNullException(nameof(options));
            this._tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            this._ownsTransport = ownsTransport;
            this._ownsTokenProvider = ownsTokenProvider;
        }

        public Uri BaseUri { get; }

        public Uri ApiBaseUri { get; }

        public IWalacorHttpClient Transport { get; }

        public WalacorHttpClientOptions Options { get; }

        public Uri GetServiceBase(string segment)
        {
            if (string.IsNullOrWhiteSpace(segment))
            {
                segment = string.Empty;
            }

            var path = this.ApiBaseUri.AbsolutePath;
            var combined = UriHelper.CombinePaths(path, segment);
            var builder = new UriBuilder(this.ApiBaseUri) { Path = combined };
            return builder.Uri;
        }

        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this._disposed = true;

            if (this._ownsTransport && this.Transport is IDisposable d1)
            {
                d1.Dispose();
            }

            if (this._ownsTokenProvider && this._tokenProvider is IDisposable d2)
            {
                d2.Dispose();
            }
        }
    }
}
