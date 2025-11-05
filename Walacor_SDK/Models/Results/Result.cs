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

namespace Walacor_SDK.Models.Results
{
    public sealed class Result<T>
    {
        private Result(bool ok, T? val, Error? err, int? status, string? corrId)
        {
            this.IsSuccess = ok;
            this.Value = val;
            this.Error = err;
            this.StatusCode = status;
            this.CorrelationId = corrId;
        }

        public bool IsSuccess { get; }

        public T? Value { get; }

        public Error? Error { get; }

        public int? StatusCode { get; }

        public string? CorrelationId { get; }

        public static Result<T> Success(T value, int? statusCode = 200, string? correlationId = null)
            => new(true, value, null, statusCode, correlationId);

        public static Result<T> Fail(Error error, int? statusCode = null, string? correlationId = null)
            => new(false, default, error, statusCode, correlationId);

        public void Deconstruct(out bool ok, out T? val, out Error? err)
        {
            ok = this.IsSuccess;
            val = this.Value;
            err = this.Error;
        }

        public TResult Match<TResult>(Func<T, TResult> onOk, Func<Error, TResult> onErr)
            => this.IsSuccess ? onOk(this.Value!) : onErr(this.Error!);

        public Result<TResult> Map<TResult>(Func<T, TResult> map)
            => this.IsSuccess
                ? Result<TResult>.Success(map(this.Value!), this.StatusCode, this.CorrelationId)
                : Result<TResult>.Fail(this.Error!, this.StatusCode, this.CorrelationId);

        public Result<T> Ensure(Func<T, bool> predicate, string errorCode, string message)
            => this.IsSuccess && !predicate(this.Value!)
                ? Fail(Error.Validation(errorCode, message), this.StatusCode, this.CorrelationId)
                : this;
    }
}
