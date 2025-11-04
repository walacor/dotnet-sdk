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
        public static Result<T> FromEnvelope<T>(
            string json,
            Func<string, T?> deserialize,
            int statusCode,
            string? correlationId)
        {
            try
            {
                var envelope = deserialize(json) as dynamic;

                if (envelope is BaseResponse<T> b)
                {
                    if (b.Success && b.Data is not null)
                    {
                        return Result<T>.Success(b.Data, statusCode, correlationId);
                    }

                    return Result<T>.Fail(Error.Server("Operation failed."), statusCode, correlationId);
                }
            }
            catch
            {
                // fall through → try plain T
            }

            try
            {
                var value = deserialize(json);
                if (value is not null)
                {
                    return Result<T>.Success(value, statusCode, correlationId);
                }

                return Result<T>.Fail(Error.Deserialization(), statusCode, correlationId);
            }
            catch
            {
                return Result<T>.Fail(Error.Deserialization(), statusCode, correlationId);
            }
        }
    }
}
