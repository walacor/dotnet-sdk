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
using System.Net.Http;
using System.Threading.Tasks;
using Walacor_SDK.Client.Exceptions;
using Walacor_SDK.Client.Pipeline;
using Walacor_SDK.Models.Result;
using Walacor_SDK.Models.Results;
using Walacor_SDK.W_Client.Constants;

namespace Walacor_SDK.W_Client.Mappers
{
    internal static class ExceptionResult
    {
        public static Result<T> From<T>(Exception ex)
        {
            if (ex is TaskCanceledException or TimeoutException)
            {
                return Result<T>.Fail(Error.Timeout(), null, ExtractCorr(ex));
            }

            if (ex is WalacorNetworkException || ex is HttpRequestException)
            {
                return Result<T>.Fail(Error.Network(), null, ExtractCorr(ex));
            }

            if (ex is WalacorAuthException aex)
            {
                return Result<T>.Fail(Error.Unauthorized(aex.Message), HttpStatusCodes.Unauthorized, ExtractCorr(aex));
            }

            if (ex is WalacorValidationException vex)
            {
                return Result<T>.Fail(
                    Error.Validation(ErrorCodes.ValidationFailed, vex.Message),
                    HttpStatusCodes.UnprocessableEntity,
                    ExtractCorr(vex));
            }

            if (ex is WalacorServerException sex)
            {
                return Result<T>.Fail(Error.Server(sex.Message), TryGetStatus(sex), ExtractCorr(sex));
            }

            if (ex is WalacorRequestException rex)
            {
                var status = TryGetStatus(rex);

                var err =
                    status == HttpStatusCodes.NotFound ? Error.NotFound()
                    : status == HttpStatusCodes.BadRequest ? Error.Validation(ErrorCodes.BadRequest, rex.Message)
                    : Error.Unknown(rex.Message);

                return Result<T>.Fail(err, status, ExtractCorr(rex));
            }

            return Result<T>.Fail(Error.Unknown(ex.Message), null, ExtractCorr(ex));
        }

        private static int? TryGetStatus(Exception ex)
        {
            return ex switch
            {
                WalacorServerException s => (int)s.StatusCode,
                WalacorRequestException r => (int)r.StatusCode,
                _ => (int?)null,
            };
        }

        private static string? ExtractCorr(Exception ex)
        {
            if (ex.Data.Contains(CorrelationLoggingHandler.CorrelationKey))
            {
                return ex.Data[CorrelationLoggingHandler.CorrelationKey]?.ToString();
            }

            return null;
        }
    }
}
