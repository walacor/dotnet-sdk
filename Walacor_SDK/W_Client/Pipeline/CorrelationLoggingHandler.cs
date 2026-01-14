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
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Walacor_SDK.W_Client.Constants;

namespace Walacor_SDK.Client.Pipeline
{
    internal sealed class CorrelationLoggingHandler(HttpMessageHandler innerHandler) : DelegatingHandler(innerHandler)
    {
        public const string CorrelationHeader = CorrelationConstants.CorrelationHeader;
        public const string CorrelationKey = CorrelationConstants.CorrelationKey;
        public const string DurationKey = CorrelationConstants.DurationKey;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken ct)
        {
            // reuse correlation id if already present; or create new one
            string correlationId;
            if (!request.Headers.TryGetValues(CorrelationConstants.CorrelationHeader, out var values))
            {
                correlationId = Guid.NewGuid().ToString(CorrelationConstants.GuidFormatCompact);
                request.Headers.Add(CorrelationConstants.CorrelationHeader, correlationId);
            }
            else
            {
                using var e = values.GetEnumerator();
                correlationId = (e.MoveNext()
                    ? e.Current
                    : Guid.NewGuid().ToString(CorrelationConstants.GuidFormatCompact))!;
            }

            request.Properties[CorrelationConstants.CorrelationKey] = correlationId;

            var sw = Stopwatch.StartNew();
            try
            {
                var response = await base.SendAsync(request, ct).ConfigureAwait(false);
                sw.Stop();

                response.Headers.TryAddWithoutValidation(CorrelationConstants.CorrelationHeader, correlationId);
                response.RequestMessage!.Properties[CorrelationConstants.DurationKey] = sw.ElapsedMilliseconds;
                response.RequestMessage!.Properties[CorrelationConstants.CorrelationKey] = correlationId;

                return response;
            }
            catch (Exception ex)
            {
                sw.Stop();

                // Attach correlation for upstream mappers
                ex.Data[CorrelationConstants.CorrelationKey] = correlationId;
                ex.Data[CorrelationConstants.DurationKey] = sw.ElapsedMilliseconds;
                throw;
            }
        }
    }
}
