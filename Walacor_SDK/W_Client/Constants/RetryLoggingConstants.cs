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
    internal static class RetryLoggingConstants
    {
        public const string MsgMaxRetriesReachedWithStatus =
           "HTTP retry limit reached after {Attempt} attempts with status {StatusCode} (CorrelationId: {CorrelationId})";

        public const string MsgRetryingWithStatus =
            "Retrying HTTP request attempt {Attempt} after {DelayMs} ms due to status {StatusCode} (CorrelationId: {CorrelationId})";

        public const string MsgMaxRetriesReachedNetworkFailure =
            "HTTP retry limit reached after {Attempt} attempts due to network failure (CorrelationId: {CorrelationId})";

        public const string MsgRetryingNetworkFailure =
            "Retrying HTTP request attempt {Attempt} after {DelayMs} ms due to network failure (CorrelationId: {CorrelationId})";

        public const string CorrelationHeader = AuthLoggingConstants.CorrelationHeader;
        public const string CorrelationPropertyKey = AuthLoggingConstants.CorrelationPropertyKey;

        public static readonly EventId RetryingRequest = new EventId(2000, nameof(RetryingRequest));
        public static readonly EventId MaxRetriesReached = new EventId(2001, nameof(MaxRetriesReached));
    }
}
