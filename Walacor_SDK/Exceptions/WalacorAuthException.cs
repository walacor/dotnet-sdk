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

namespace Walacor_SDK.Exceptions
{
    public sealed class WalacorAuthException(
        string message,
        string? correlationId = null,
        Exception? inner = null)
        : Exception(message, inner)
    {
        public string? CorrelationId { get; } = correlationId;
    }
}


/*
class APIConnectionError(Exception):
       """Raised when unable to connect to the Walacor API."""

   class BadRequestError(Exception):
       def __init__(self, reason: str, message: str, code: int = 400):
           self.reason = reason
           self.message = message
           self.code = code
           super().__init__(f"[{reason}] {message}")

   class FileRequestError(RuntimeError):
       """Raised when a file‐service operation fails."""

   class DuplicateFileError(FileRequestError):
       """Raised when the platform reports the file is a duplicate."""
*/
