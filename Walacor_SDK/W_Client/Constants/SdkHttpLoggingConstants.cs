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

using Microsoft.Extensions.Logging;

namespace Walacor_SDK.W_Client.Constants
{
    internal static class SdkHttpLoggingConstants
    {
        public const string MsgRequestStarted =
            "HTTP request started {Method} {Path} (CorrelationId: {CorrelationId})";

        public const string MsgRequestHeaders =
            "HTTP request headers {Method} {Path} (CorrelationId: {CorrelationId}){Headers}";

        public const string MsgRequestBody =
            "HTTP request body {Method} {Path} (CorrelationId: {CorrelationId}){Body}";

        public const string MsgRequestCompleted =
            "HTTP request completed {Method} {Path} with {StatusCode} in {DurationMs} ms (CorrelationId: {CorrelationId})";

        public const string MsgResponseHeaders =
            "HTTP response headers {Method} {Path} (CorrelationId: {CorrelationId}){Headers}";

        public const string MsgResponseBody =
            "HTTP response body {Method} {Path} (CorrelationId: {CorrelationId}){Body}";

        public const string MsgRequestFailed =
            "HTTP request failed {Method} {Path} in {DurationMs} ms (CorrelationId: {CorrelationId})";

        public const string CorrelationHeader = AuthLoggingConstants.CorrelationHeader;
        public const string CorrelationPropertyKey = AuthLoggingConstants.CorrelationPropertyKey;

        public static readonly EventId HttpRequestStart = new EventId(1000, nameof(HttpRequestStart));
        public static readonly EventId HttpRequestHeaders = new EventId(1001, nameof(HttpRequestHeaders));
        public static readonly EventId HttpRequestBody = new EventId(1002, nameof(HttpRequestBody));
        public static readonly EventId HttpRequestStop = new EventId(1003, nameof(HttpRequestStop));
        public static readonly EventId HttpResponseHeaders = new EventId(1004, nameof(HttpResponseHeaders));
        public static readonly EventId HttpResponseBody = new EventId(1005, nameof(HttpResponseBody));
        public static readonly EventId HttpRequestException = new EventId(1006, nameof(HttpRequestException));
    }
}
