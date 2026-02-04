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
    internal static class AuthLoggingConstants
    {
        public const string MsgTokenRefreshSucceeded =
         "Token refresh succeeded (CorrelationId: {CorrelationId}, DurationMs: {DurationMs})";

        public const string MsgTokenRefreshCancelled =
            "Token refresh cancelled (CorrelationId: {CorrelationId}, DurationMs: {DurationMs})";

        public const string MsgTokenRefreshFailedWithDuration =
            "Token refresh failed (CorrelationId: {CorrelationId}, DurationMs: {DurationMs})";

        public const string MsgRetryAfterRefreshCompleted =
            "Retry after token refresh completed (CorrelationId: {CorrelationId}, Status: {StatusCode}, DurationMs: {DurationMs})";

        public const string MsgRetryAfterRefreshCancelled =
            "Retry after token refresh cancelled (CorrelationId: {CorrelationId}, DurationMs: {DurationMs})";

        public const string MsgRetryAfterRefreshFailed =
            "Retry after token refresh failed (CorrelationId: {CorrelationId}, DurationMs: {DurationMs})";

        public const string MsgRefreshingToken =
            "401 received, refreshing token (CorrelationId: {CorrelationId})";

        public const string MsgTokenRefreshFailed =
            "Token refresh failed (CorrelationId: {CorrelationId})";

        public const string MsgRetryingAfterRefresh =
            "Retrying request after token refresh (CorrelationId: {CorrelationId})";

        public const string CorrelationHeader = "X-Correlation-Id";

        public const string CorrelationPropertyKey = "Walacor.CorrelationId";

        public static readonly EventId RefreshingToken = new EventId(3000, nameof(RefreshingToken));
        public static readonly EventId TokenRefreshFailed = new EventId(3001, nameof(TokenRefreshFailed));
        public static readonly EventId RetryingAfterRefresh = new EventId(3002, nameof(RetryingAfterRefresh));
        public static readonly EventId TokenRefreshSucceeded = new EventId(2005, nameof(TokenRefreshSucceeded));
        public static readonly EventId TokenRefreshCancelled = new EventId(2006, nameof(TokenRefreshCancelled));
        public static readonly EventId RetryAfterRefreshCompleted = new EventId(2007, nameof(RetryAfterRefreshCompleted));
        public static readonly EventId RetryAfterRefreshCancelled = new EventId(2008, nameof(RetryAfterRefreshCancelled));
        public static readonly EventId RetryAfterRefreshFailed = new EventId(2009, nameof(RetryAfterRefreshFailed));
    }
}
