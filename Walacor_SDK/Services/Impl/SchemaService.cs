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
using System.Threading;
using System.Threading.Tasks;
using Walacor_SDK.Models.Results;
using Walacor_SDK.Services.Abs;
using Walacor_SDK.W_Client.Context;

namespace Walacor_SDK.Services.Impl
{
    internal sealed class SchemaService : ISchemaService
    {
        private readonly ClientContext _ctx;
        private readonly string _segment;

        public SchemaService(ClientContext ctx, string segment = "schemas")
        {
            this._ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
            this._segment = string.IsNullOrWhiteSpace(segment) ? "schemas" : segment.Trim('/');
        }

        public async Task<Result<IReadOnlyList<string>>> GetDataTypesAsync(CancellationToken ct = default)
        {
            var path = $"{this._segment}/dataTypes";

            // transport already returns Result<List<string>>
            var res = await this._ctx.Transport
                .GetJsonAsync<List<string>>(path, query: null, ct)
                .ConfigureAwait(false);

            // Optional: normalize to read-only and enforce non-empty, with your own error code/message
            return res
                .Map(list => (IReadOnlyList<string>)list.AsReadOnly())
                .Ensure(list => list.Count > 0, "empty_list", "No data types were returned.");
        }
    }
}
