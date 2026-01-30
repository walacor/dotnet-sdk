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
using Microsoft.Extensions.Logging.Abstractions;
using Walacor_SDK.Client.Pipeline;
using Walacor_SDK.Client.Serialization;
using Walacor_SDK.Client.Strategies;
using Walacor_SDK.W_Client.Abstractions;
using Walacor_SDK.W_Client.Options;

namespace Walacor_SDK.Client.Transport
{
    internal sealed class HttpPipelineFactory
    {
        internal HttpClient Create(
            Uri baseAddress,
            IAuthTokenProvider tokens,
            WalacorHttpClientOptions options,
            IBackoffStrategy? backoff = null,
            int maxRetries = 2)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            // Transport
            var transport = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseCookies = false,
            };

            // Chain: Correlation -> SDK Logging -> Retry -> Auth -> Transport
            var loggerFactory = options.LoggerFactory ?? NullLoggerFactory.Instance;
            var authLogger = loggerFactory.CreateLogger("Walacor_SDK.Auth");
            var retryLogger = loggerFactory.CreateLogger("Walacor_SDK.Retry");
            var httpLogger = loggerFactory.CreateLogger("Walacor_SDK.Http");

            var auth = new AuthHandler(tokens, authLogger, transport);
            var retry = new RetryHandler(backoff ?? new ExponentialJitterBackoff(), maxRetries, retryLogger, auth);
            var sdkLogging = new SdkLoggingHandler(httpLogger, options, retry);
            var logging = new CorrelationLoggingHandler(sdkLogging);

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
