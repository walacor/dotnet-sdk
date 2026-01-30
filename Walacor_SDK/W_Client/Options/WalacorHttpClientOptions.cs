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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Walacor_SDK.W_Client.Options
{
    public sealed class WalacorHttpClientOptions
    {
        public int MaxRetries { get; set; } = 2;

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(2);

        public bool ThrowOnValidation422 { get; set; } = true;

        public ILoggerFactory LoggerFactory { get; set; } = NullLoggerFactory.Instance;

        public bool LogRequestHeaders { get; set; }

        public bool LogResponseHeaders { get; set; }

        public bool LogBodies { get; set; }

        public Func<string, bool> RedactHeader { get; set; } = DefaultRedactHeader;

        public LogLevel SuccessLevel { get; set; } = LogLevel.Information;

        public LogLevel FailureLevel { get; set; } = LogLevel.Warning;

        public LogLevel ExceptionLevel { get; set; } = LogLevel.Error;

        private static bool DefaultRedactHeader(string headerName)
        {
            if (string.IsNullOrWhiteSpace(headerName))
            {
                return false;
            }

            return headerName.Equals("Authorization", StringComparison.OrdinalIgnoreCase)
                   || headerName.Equals("Cookie", StringComparison.OrdinalIgnoreCase)
                   || headerName.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)
                   || headerName.Equals("X-Api-Key", StringComparison.OrdinalIgnoreCase);
        }
    }
}
