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
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Walacor_SDK.Enums;
using Walacor_SDK.Models.Results;
using Walacor_SDK.Models.Schema.Request;
using Walacor_SDK.Models.Schema.Response;
using Walacor_SDK.Services.Abs;
using Walacor_SDK.W_Client.Context;
using Walacor_SDK.W_Client.Helpers;

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

        public async Task<Result<IReadOnlyList<DataTypeDto>>> GetDataTypesAsync(CancellationToken ct = default)
        {
            var path = $"{this._segment}/dataTypes";

            var res = await this._ctx.Transport
                .GetJsonAsync<List<DataTypeDto>>(path, query: null, ct)
                .ConfigureAwait(false);

            return res.Map(list => (IReadOnlyList<DataTypeDto>)list.AsReadOnly());
        }

        public async Task<Result<IReadOnlyDictionary<string, AutoGenFieldDto>>> GetPlatformAutoGenerationFieldsAsync(
         CancellationToken ct = default)
        {
            var path = $"{this._segment}/systemFields";

            var res = await this._ctx.Transport
                .GetJsonAsync<Dictionary<string, AutoGenFieldDto>>(path, query: null, ct)
                .ConfigureAwait(false);

            return res.Map(dict =>
                (IReadOnlyDictionary<string, AutoGenFieldDto>)new ReadOnlyDictionary<string, AutoGenFieldDto>(dict));
        }

        public async Task<Result<IReadOnlyList<SchemaEntryDto>>> GetListWithLatestVersionAsync(CancellationToken ct = default)
        {
            var path = $"{this._segment}/versions/latest";

            var res = await this._ctx.Transport
                .GetJsonAsync<List<SchemaEntryDto>>(path, query: null, ct)
                .ConfigureAwait(false);

            return res.Map(list => (IReadOnlyList<SchemaEntryDto>)list.AsReadOnly());
        }

        public async Task<Result<IReadOnlyList<SchemaVersionEntryDto>>> GetVersionsAsync(CancellationToken ct = default)
        {
            var path = $"{this._segment}/versions";

            var res = await this._ctx.Transport
                .GetJsonAsync<List<SchemaVersionEntryDto>>(path, query: null, ct)
                .ConfigureAwait(false);

            return res.Map(list => (IReadOnlyList<SchemaVersionEntryDto>)list.AsReadOnly());
        }

        public async Task<Result<IReadOnlyList<int>>> GetVersionsForEnvelopeTypeAsync(int etId, CancellationToken ct = default)
        {
            var path = $"{this._segment}/envelopeTypes/{etId}/versions";
            var res = await this._ctx.Transport
               .GetJsonAsync<List<int>>(path, query: null, ct)
               .ConfigureAwait(false);

            return res.Map(list => (IReadOnlyList<int>)list.AsReadOnly());
        }

        public async Task<Result<IReadOnlyList<IndexEntryDto>>> GetIndexesAsync(int etId, CancellationToken ct = default)
        {
            var path = $"{this._segment}/envelopeTypes/{etId}/indexes";
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["ETId"] = etId.ToString(System.Globalization.CultureInfo.InvariantCulture),
            };

            var res = await this._ctx.Transport
               .GetJsonWithHeadersAsync<List<IndexEntryDto>>(path, query: null, headers, ct)
               .ConfigureAwait(false);

            return res.Map(list => (IReadOnlyList<IndexEntryDto>)list.AsReadOnly());
        }

        public async Task<Result<IReadOnlyList<IndexEntryDto>>> GetIndexesAsync(SystemEnvelopeType etId, CancellationToken ct = default)
        {
            var path = $"{this._segment}/envelopeTypes/{etId}/indexes";

            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["ETId"] = EnumHelper.EtIdToString(etId),
            };

            var response = await this._ctx.Transport
                .GetJsonWithHeadersAsync<List<IndexEntryDto>>(path, query: null, headers, ct)
                .ConfigureAwait(false);

            return response.Map(list => (IReadOnlyList<IndexEntryDto>)list.AsReadOnly());
        }

        public async Task<Result<IReadOnlyList<IndexEntryDto>>> GetIndexesByTableNameAsync(string tableName, CancellationToken ct = default)
        {
            var path = $"{this._segment}/envelopeTypes/{15}/indexesByTableName";

            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["ETId"] = "15",
            };

            var query = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["tableName"] = tableName,
            };

            var response = await this._ctx.Transport
                .GetJsonWithHeadersAsync<List<IndexEntryDto>>(path, query, headers, ct)
                .ConfigureAwait(false);

            return response.Map(list => (IReadOnlyList<IndexEntryDto>)list.AsReadOnly());
        }

        public async Task<Result<SchemaMetadataDto>> CreateSchemaAsync(CreateSchemaRequest request, CancellationToken ct = default)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var path = $"{this._segment}";

            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["ETId"] = "15",
                ["SV"] = "1",
            };

            var res = await this._ctx.Transport
                .PostJsonWithHeadersAsync<CreateSchemaRequest, SchemaMetadataDto>(
                    path,
                    request,
                    query: null,
                    headers: headers,
                    ct: ct)
                .ConfigureAwait(false);

            return res;
        }

        public async Task<Result<SchemaDetailDto>> GetSchemaDetailsByEnvelopeTypeAsync(int etId, CancellationToken ct = default)
        {
            var path = $"{this._segment}/envelopeTypes/{etId}/details";

            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["ETid"] = etId.ToString(System.Globalization.CultureInfo.InvariantCulture),
            };

            var response = await this._ctx.Transport
                .GetJsonWithHeadersAsync<SchemaDetailDto>(path, query: null, headers, ct)
                .ConfigureAwait(false);

            return response;
        }

        public async Task<Result<IReadOnlyList<long>>> GetEnvelopeTypesAsync(CancellationToken ct = default)
        {
            var path = $"{this._segment}/envelopeTypes";

            var res = await this._ctx.Transport
                .GetJsonAsync<List<long>>(path, query: null, ct)
                .ConfigureAwait(false);

            return res.Map(list => (IReadOnlyList<long>)list.AsReadOnly());
        }

        public async Task<Result<SchemaDetailDto>> GetDetailsByIdAsync(string id, CancellationToken ct = default)
        {
            var path = $"{this._segment}/{id}";

            var res = await this._ctx.Transport
                .GetJsonAsync<SchemaDetailDto>(path, query: null, ct)
                .ConfigureAwait(false);

            return res;
        }

        public async Task<Result<IReadOnlyList<SchemaItemDto>>> GetListSchemaItemsAsync(CancellationToken ct = default)
        {
            var path = $"{this._segment}";

            var res = await this._ctx.Transport
                 .GetJsonAsync<List<SchemaItemDto>>(path, query: null, ct)
                 .ConfigureAwait(false);

            return res.Map(list => (IReadOnlyList<SchemaItemDto>)list.AsReadOnly());
        }

        public async Task<Result<Paged<SchemaSummaryDto>>> GetSchemaQuerySchemaItemsAsync(
            SchemaQueryListRequest request,
            CancellationToken ct = default)
        {
            var path = $"{this._segment}/schemaList";

            var query = QueryHelper.BuildQueryFromObject(request);

            var res = await this._ctx.Transport
                .GetJsonAsync<List<SchemaSummaryDto>>(path, query, ct)
                .ConfigureAwait(false);

            var items = (res.Value ?? new List<SchemaSummaryDto>()).AsReadOnly();

            return Result<Paged<SchemaSummaryDto>>.Success(
                new Paged<SchemaSummaryDto>(items, items.Count),
                res.StatusCode,
                res.CorrelationId,
                res.DurationMs);
        }
    }
}
