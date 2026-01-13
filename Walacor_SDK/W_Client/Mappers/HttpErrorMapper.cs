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

using Newtonsoft.Json.Linq;
using Walacor_SDK.Models.FileRequests.Response; // <-- DuplicateData
using Walacor_SDK.Models.Result;

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
                400 => Error.Validation("bad_request", "The request was invalid."),
                401 => Error.Unauthorized(),
                404 => Error.NotFound(),
                408 => Error.Timeout(),
                429 => Error.Server("Too many requests."),
                >= 500 and < 600 => Error.Server($"Server error ({statusCode})."),
                _ => Error.Unknown($"HTTP {statusCode}. {Trim(body ?? string.Empty)}"),
            };
        }

        private static Error? TryParseStructuredError(int statusCode, string body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                return null;
            }

            // Some servers prepend non-json text; trim to first '{'
            var idx = body.IndexOf('{');
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

                var bodyCode = obj["code"]?.Value<int?>();
                var errors = obj["errors"] as JArray;

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
            if (effective != 422)
            {
                return null;
            }

            if (obj["duplicateData"] is not JObject dupObj)
            {
                return null;
            }

            var dup = dupObj.ToObject<DuplicateData>();
            if (dup is null || dup.UIDs is null || dup.UIDs.Count == 0)
            {
                return null;
            }

            var err = Error.Validation("duplicate_file", "Duplicate file detected.");

            // One strong object, not many fields
            err.Details["duplicateData"] = dup;

            // diagnostics
            err.Details["rawBody"] = Trim(body);
            if (bodyCode.HasValue)
            {
                err.Details["code"] = bodyCode.Value;
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

            var reason = first["reason"]?.Value<string>();
            var message = first["message"]?.Value<string>();

            var effective = bodyCode ?? statusCode;
            var error = MapByStatus(effective, reason, message);

            if (!string.IsNullOrEmpty(reason))
            {
                error.Details["reason"] = reason;
            }

            error.Details["rawBody"] = Trim(body);
            if (bodyCode.HasValue)
            {
                error.Details["code"] = bodyCode.Value;
            }

            return error;
        }

        private static Error? TryMapSingleError(int statusCode, string body, int? bodyCode, JObject obj)
        {
            var singleError = obj["error"]?.Value<string>();
            if (string.IsNullOrWhiteSpace(singleError))
            {
                return null;
            }

            var effective = bodyCode ?? statusCode;
            var error = MapByStatus(effective, "serverError", singleError);

            error.Details["rawBody"] = Trim(body);
            if (bodyCode.HasValue)
            {
                error.Details["code"] = bodyCode.Value;
            }

            return error;
        }

        private static Error MapByStatus(int statusCode, string? reason, string? message)
        {
            var code = string.IsNullOrWhiteSpace(reason) ? "error" : reason!;
            var msg = string.IsNullOrWhiteSpace(message) ? "Operation failed." : message!;

            return statusCode switch
            {
                400 => Error.Validation(code, msg),
                401 => Error.Unauthorized(msg),
                404 => Error.NotFound(msg),
                >= 500 and < 600 => Error.Server(msg),
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
            return value.Length > 300 ? value.Substring(0, 300) + "…" : value;
        }
    }
}
