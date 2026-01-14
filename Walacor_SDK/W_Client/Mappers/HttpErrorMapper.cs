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

using System.Globalization;
using Newtonsoft.Json.Linq;
using Walacor_SDK.Models.FileRequests.Response;
using Walacor_SDK.Models.Result;
using Walacor_SDK.W_Client.Constants;

namespace Walacor_SDK.W_Client.Mappers
{
    internal static class HttpErrorMapper
    {
        public static Error FromStatus(int statusCode, string? body)
        {
            var parsed = TryParseStructuredError(statusCode, body ?? string.Empty);
            if (parsed is not null)
            {
                return parsed;
            }

            return statusCode switch
            {
                HttpStatusCodes.BadRequest => Error.Validation(ErrorCodes.BadRequest, HttpErrorMapperMessages.RequestInvalid),
                HttpStatusCodes.Unauthorized => Error.Unauthorized(),
                HttpStatusCodes.NotFound => Error.NotFound(),
                HttpStatusCodes.RequestTimeout => Error.Timeout(),
                HttpStatusCodes.TooManyRequests => Error.Server(HttpErrorMapperMessages.TooManyRequests),
                >= HttpStatusRanges.ServerErrorMinInclusive and < HttpStatusRanges.ServerErrorMaxExclusive
                => Error.Server(string.Format(CultureInfo.InvariantCulture, HttpErrorMapperMessages.ServerErrorWithStatusCodeFormat, statusCode)),
                _ => Error.Unknown(string.Format(
                        CultureInfo.InvariantCulture,
                        HttpErrorMapperMessages.UnknownHttpWithBodyFormat,
                        statusCode,
                        Trim(body ?? string.Empty))),
            };
        }

        private static Error? TryParseStructuredError(int statusCode, string body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                return null;
            }

            // Some servers prepend non-json text; trim to first '{'
            var idx = body.IndexOf(HttpErrorMapperConstants.JsonObjectStartChar);
            if (idx > 0)
            {
                body = body.Substring(idx);
            }

            try
            {
                var root = JToken.Parse(body);
                if (root is not JObject obj)
                {
                    return null;
                }

                var bodyCode = obj[JsonPropertyNames.Code]?.Value<int?>();
                var errors = obj[JsonPropertyNames.Errors] as JArray;

                // Shape 1: { "errors": [ { "reason": "...", "message": "..." } ], "code": ... }
                var fromArray = TryMapErrorsArray(statusCode, body, bodyCode, errors);
                if (fromArray is not null)
                {
                    return fromArray;
                }

                // Shape 2 (verify duplicate): { "success": false, "duplicateData": { ... } }
                var dup = TryMapDuplicateData(statusCode, body, bodyCode, obj);
                if (dup is not null)
                {
                    return dup;
                }

                // Shape 3: { "error": "..." }
                return TryMapSingleError(statusCode, body, bodyCode, obj);
            }
            catch
            {
                return null;
            }
        }

        private static Error? TryMapDuplicateData(int statusCode, string body, int? bodyCode, JObject obj)
        {
            var effective = bodyCode ?? statusCode;
            if (effective != HttpStatusCodes.UnprocessableEntity)
            {
                return null;
            }

            if (obj[JsonPropertyNames.DuplicateData] is not JObject dupObj)
            {
                return null;
            }

            var dup = dupObj.ToObject<DuplicateData>();
            if (dup is null || dup.UIDs is null || dup.UIDs.Count == 0)
            {
                return null;
            }

            var err = Error.Validation(ErrorCodes.DuplicateFile, HttpErrorMapperMessages.DuplicateFileDetected);

            // One strong object, not many fields
            err.Details[ErrorDetailKeys.DuplicateData] = dup;

            // diagnostics
            err.Details[ErrorDetailKeys.RawBody] = Trim(body);
            if (bodyCode.HasValue)
            {
                err.Details[ErrorDetailKeys.Code] = bodyCode.Value;
            }

            return err;
        }

        private static Error? TryMapErrorsArray(int statusCode, string body, int? bodyCode, JArray? errors)
        {
            if (errors is null || errors.Count == 0)
            {
                return null;
            }

            if (errors[0] is not JObject first)
            {
                return null;
            }

            var reason = first[JsonPropertyNames.Reason]?.Value<string>();
            var message = first[JsonPropertyNames.Message]?.Value<string>();

            var effective = bodyCode ?? statusCode;
            var error = MapByStatus(effective, reason, message);

            if (!string.IsNullOrEmpty(reason))
            {
                error.Details[ErrorDetailKeys.Reason] = reason;
            }

            error.Details[ErrorDetailKeys.RawBody] = Trim(body);
            if (bodyCode.HasValue)
            {
                error.Details[ErrorDetailKeys.Code] = bodyCode.Value;
            }

            return error;
        }

        private static Error? TryMapSingleError(int statusCode, string body, int? bodyCode, JObject obj)
        {
            var singleError = obj[JsonPropertyNames.Error]?.Value<string>();
            if (string.IsNullOrWhiteSpace(singleError))
            {
                return null;
            }

            var effective = bodyCode ?? statusCode;
            var error = MapByStatus(effective, HttpErrorMapperReasonCodes.ServerError, singleError);

            error.Details[ErrorDetailKeys.RawBody] = Trim(body);
            if (bodyCode.HasValue)
            {
                error.Details[ErrorDetailKeys.Code] = bodyCode.Value;
            }

            return error;
        }

        private static Error MapByStatus(int statusCode, string? reason, string? message)
        {
            var code = string.IsNullOrWhiteSpace(reason) ? HttpErrorMapperReasonCodes.DefaultError : reason!;
            var msg = string.IsNullOrWhiteSpace(message) ? HttpErrorMapperMessages.OperationFailed : message!;

            return statusCode switch
            {
                HttpStatusCodes.BadRequest => Error.Validation(code, msg),
                HttpStatusCodes.Unauthorized => Error.Unauthorized(msg),
                HttpStatusCodes.NotFound => Error.NotFound(msg),
                >= HttpStatusRanges.ServerErrorMinInclusive and < HttpStatusRanges.ServerErrorMaxExclusive => Error.Server(msg),
                _ => Error.Unknown(msg),
            };
        }

        private static string Trim(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return string.Empty;
            }

            var value = s.Trim();
            return value.Length > HttpErrorMapperConstants.TrimMaxLength
                ? value.Substring(0, HttpErrorMapperConstants.TrimMaxLength) + HttpErrorMapperConstants.Ellipsis
                : value;
        }
    }
}
