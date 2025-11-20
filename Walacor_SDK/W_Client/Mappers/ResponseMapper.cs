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
using Newtonsoft.Json.Linq;
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

                if (env is null)
                {
                    return Result<T>.Fail(
                        Error.Deserialization("Invalid envelope format."),
                        statusCode,
                        correlationId,
                        durationMs);
                }

                if (env.Success)
                {
                    if (env.Data is null)
                    {
                        return Result<T>.Fail(
                            Error.Deserialization("Envelope is successful but 'Data' is null."),
                            statusCode,
                            correlationId,
                            durationMs);
                    }

                    return Result<T>.Success(env.Data, statusCode, correlationId, durationMs);
                }

                var (err, logicalStatus) = MapEnvelopeError(json, statusCode);

                return Result<T>.Fail(err, logicalStatus, correlationId, durationMs);
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

        private static (Error Error, int? StatusCode) MapEnvelopeError(string json, int httpStatus)
        {
            try
            {
                var root = JToken.Parse(json);
                var errorToken = root["error"];
                if (errorToken is null || errorToken.Type == JTokenType.Null)
                {
                    return (Error.Server("Operation failed."), httpStatus);
                }

                return CreateErrorFromEnvelopeToken(errorToken, httpStatus);
            }
            catch
            {
                return (Error.Server("Operation failed."), httpStatus);
            }
        }

        private static (Error Error, int? StatusCode) CreateErrorFromEnvelopeToken(JToken errorToken, int httpStatus)
        {
            var code = errorToken["code"]?.Value<int?>();
            var errorsArray = errorToken["errors"] as JArray;

            string? reason = null;
            string? message = null;

            if (errorsArray is not null && errorsArray.Count > 0 && errorsArray[0] is JObject first)
            {
                reason = first["reason"]?.Value<string>();
                message = first["message"]?.Value<string>();
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                message = "Operation failed.";
            }

            var effectiveStatus = code ?? httpStatus;

            Error err = effectiveStatus switch
            {
                400 => Error.Validation(reason ?? "bad_request", message!),
                401 => Error.Unauthorized(message!),
                404 => Error.NotFound(message!),
                >= 500 and < 600 => Error.Server(message!),
                _ => Error.Unknown(message!),
            };

            if (!string.IsNullOrEmpty(reason))
            {
                err.Details["reason"] = reason;
            }

            if (errorsArray is not null)
            {
                err.Details["errors"] = errorsArray.ToObject<object?[]?>();
            }

            if (code.HasValue)
            {
                err.Details["code"] = code.Value;
            }

            if (code.HasValue && code.Value != httpStatus)
            {
                err.Details["httpStatus"] = httpStatus;
            }

            return (err, effectiveStatus);
        }
    }
}
