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
using System.Linq;
using Walacor_SDK.Models.Results;

namespace Walacor_SDK.Models.Result
{
    public sealed class Error
    {
        public Error(string code, string message, string? target = null)
        {
            this.Code = code ?? throw new ArgumentNullException(nameof(code));
            this.Message = message ?? string.Empty;
            this.Target = target;
        }

        public string Code { get; }

        public string Message { get; }

        public string? Target { get; }

        public IDictionary<string, object?> Details { get; } =
            new Dictionary<string, object?>(StringComparer.Ordinal);

        public IList<ValidationError> ValidationErrors { get; } =
            new List<ValidationError>();

        // Blank line separating elements

        // Factory helpers
        public static Error Validation(string code = "validation_failed", string message = "Validation failed.")
        {
            return new Error(code, message);
        }

        public static Error Unauthorized(string message = "Unauthorized")
        {
            return new Error("unauthorized", message);
        }

        public static Error NotFound(string message = "Not Found")
        {
            return new Error("not_found", message);
        }

        public static Error Server(string message = "Server error")
        {
            return new Error("server_error", message);
        }

        public static Error Network(string message = "Network failure")
        {
            return new Error("network_error", message);
        }

        public static Error Timeout(string message = "Request timed out")
        {
            return new Error("timeout", message);
        }

        public static Error Deserialization(string message = "Invalid response payload")
        {
            return new Error("deserialization_error", message);
        }

        public static Error Unknown(string message = "Unknown error")
        {
            return new Error("unknown_error", message);
        }

        // Methods after properties/factories
        public string ToUserMessage()
        {
            if (this.ValidationErrors.Count > 0)
            {
                var bullets = string.Join(
                    "\n• ",
                    this.ValidationErrors.Select(v => $"{v.Field}: {v.Message}"));

                return $"{this.Message}\n• {bullets}";
            }

            return this.Message;
        }
    }
}
