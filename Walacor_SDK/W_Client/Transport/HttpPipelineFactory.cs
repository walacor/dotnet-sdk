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
using Walacor_SDK.Client.Pipeline;
using Walacor_SDK.Client.Serialization;
using Walacor_SDK.Client.Strategies;
using Walacor_SDK.W_Client.Abstractions;

namespace Walacor_SDK.Client.Transport
{
    internal sealed class HttpPipelineFactory
    {
        internal HttpClient Create(
            Uri baseAddress,
            IAuthTokenProvider tokens,
            IBackoffStrategy? backoff = null,
            int maxRetries = 2)
        {
            // Transport
            var transport = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseCookies = false,
            };

            // Chain: Logging -> Retry -> Auth -> Transport
            var auth = new AuthHandler(tokens, transport);
            var retry = new RetryHandler(backoff ?? new ExponentialJitterBackoff(), maxRetries, auth);
            var logging = new CorrelationLoggingHandler(retry);

            var http = new HttpClient(logging)
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(100),
            };

            return http;
        }

        internal IJsonSerializer CreateJsonSerializer() => new NewtonsoftJsonSerializer();
    }
}
