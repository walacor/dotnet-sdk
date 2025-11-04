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
using Walacor_SDK.Client;
using Walacor_SDK.Helpers;
using Walacor_SDK.Services.Abs;
using Walacor_SDK.Services.Impl;
using Walacor_SDK.W_Client.Auth;
using Walacor_SDK.W_Client.Context;
using Walacor_SDK.W_Client.Factory;
using Walacor_SDK.W_Client.Options;

namespace Walacor_SDK
{
    public class WalacorService
    {
        private readonly ClientContext _context;
        private readonly ServiceFactory _factory;
        private bool _disposed;

        public WalacorService(string baseUri, string userName, string password, WalacorHttpClientOptions? options = null)
        {
            if (string.IsNullOrWhiteSpace(baseUri))
            {
                throw new ArgumentNullException(nameof(baseUri));
            }

            if (userName == null)
            {
                throw new ArgumentNullException(nameof(userName));
            }

            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            var normalizedBase = UriHelper.NormalizeBaseUri(baseUri);
            var apiBase = UriHelper.BuildApiBaseUri(normalizedBase, "api/");

            var tokenProvider = new UsernamePasswordTokenProvider(normalizedBase, userName, password);
            var transport = new WalacorHttpClient(apiBase, tokenProvider, options);

            this._context = new ClientContext(
                baseUri: normalizedBase,
                apiBaseUri: apiBase,
                transport: transport,
                options: options ?? new WalacorHttpClientOptions(),
                tokenProvider: tokenProvider,
                ownsTransport: true,
                ownsTokenProvider: true);

            this._factory = new ServiceFactory(this._context);
        }

        public ISchemaService SchemaService => this._factory.Get(ctx => new SchemaService(ctx, "schemas"));

        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this._disposed = true;
            this._context.Dispose();
        }
    }
}
