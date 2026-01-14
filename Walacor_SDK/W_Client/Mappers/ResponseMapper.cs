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
using Walacor_SDK.W_Client.Constants;

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
                        Error.Deserialization(ResponseMapperMessages.InvalidEnvelopeFormat),
                        statusCode,
                        correlationId,
                        durationMs);
                }

                if (env.Success)
                {
                    if (env.Data is null)
                    {
                        return Result<T>.Fail(
                            Error.Deserialization(ResponseMapperMessages.EnvelopeSuccessButDataNull),
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
                    Error.Deserialization(ResponseMapperMessages.InvalidEnvelopeFormat),
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
                var errorToken = root[JsonPropertyNames.Error];

                if (errorToken is null || errorToken.Type == JTokenType.Null)
                {
                    return (Error.Server(ResponseMapperMessages.OperationFailed), httpStatus);
                }

                return CreateErrorFromEnvelopeToken(errorToken, httpStatus);
            }
            catch
            {
                return (Error.Server(ResponseMapperMessages.OperationFailed), httpStatus);
            }
        }

        private static (Error Error, int? StatusCode) CreateErrorFromEnvelopeToken(JToken errorToken, int httpStatus)
        {
            var code = errorToken[JsonPropertyNames.Code]?.Value<int?>();
            var errorsArray = errorToken[JsonPropertyNames.Errors] as JArray;

            string? reason = null;
            string? message = null;

            if (errorsArray is not null && errorsArray.Count > 0 && errorsArray[0] is JObject first)
            {
                reason = first[JsonPropertyNames.Reason]?.Value<string>();
                message = first[JsonPropertyNames.Message]?.Value<string>();
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                message = ResponseMapperMessages.OperationFailed;
            }

            var effectiveStatus = code ?? httpStatus;

            Error err = effectiveStatus switch
            {
                HttpStatusCodes.BadRequest => Error.Validation(reason ?? ErrorCodes.BadRequest, message!),
                HttpStatusCodes.Unauthorized => Error.Unauthorized(message!),
                HttpStatusCodes.NotFound => Error.NotFound(message!),
                >= HttpStatusRanges.ServerErrorMinInclusive and < HttpStatusRanges.ServerErrorMaxExclusive => Error.Server(message!),
                _ => Error.Unknown(message!),
            };

            if (!string.IsNullOrEmpty(reason))
            {
                err.Details[ErrorDetailKeys.Reason] = reason;
            }

            if (errorsArray is not null)
            {
                err.Details[ErrorDetailKeys.Errors] = errorsArray.ToObject<object?[]?>();
            }

            if (code.HasValue)
            {
                err.Details[ErrorDetailKeys.Code] = code.Value;
            }

            if (code.HasValue && code.Value != httpStatus)
            {
                err.Details[ErrorDetailKeys.HttpStatus] = httpStatus;
            }

            return (err, effectiveStatus);
        }
    }
}
