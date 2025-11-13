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
using System.Text;
using Walacor_SDK.Models.Result;

namespace Walacor_SDK.W_Client.Mappers
{
    internal static class HttpErrorMapper
    {
        public static Error FromStatus(int statusCode, string? body)
        {
            return statusCode switch
            {
                400 => Error.Validation("bad_request", "The request was invalid."),
                401 => Error.Unauthorized(),
                404 => Error.NotFound(),
                408 => Error.Timeout(),
                429 => Error.Server("Too many requests."),
                >= 500 and < 600 => Error.Server($"Server error ({statusCode})."),
                _ => Error.Unknown($"HTTP {statusCode}. {Trim(body)}"),
            };
        }

        private static string Trim(string? s)
            => string.IsNullOrWhiteSpace(s) ? string.Empty : s!.Length > 300 ? s.Substring(0, 300) + "…" : s;
    }
}
