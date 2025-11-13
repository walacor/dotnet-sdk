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
using Walacor_SDK.Models.Result;
using Walacor_SDK.Models.Results;

namespace Walacor_SDK.W_Client.Mappers
{
    internal static class ResponseMapper
    {
        public static Result<T> FromSuccessEnvelope<T>(
            string json,
            Func<string, BaseResponse<T>?> deserializeEnvelope,
            int statusCode,
            string? correlationId,
            long? durationMs = null)
        {
            try
            {
                var env = deserializeEnvelope(json);
                if (env != null && env.Success && env.Data != null)
                {
                    return Result<T>.Success(env.Data, statusCode, correlationId, durationMs);
                }

                return Result<T>.Fail(
                    Error.Deserialization("Envelope missing data or not successful."),
                    statusCode,
                    correlationId,
                    durationMs);
            }
            catch
            {
                return Result<T>.Fail(
                    Error.Deserialization("Invalid envelope format."),
                    statusCode,
                    correlationId,
                    durationMs);
            }
        }
    }
}
