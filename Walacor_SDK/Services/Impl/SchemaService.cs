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
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Walacor_SDK.Enums;
using Walacor_SDK.Models.Result;
using Walacor_SDK.Models.Results;
using Walacor_SDK.Models.Schema.Request;
using Walacor_SDK.Models.Schema.Response;
using Walacor_SDK.Services.Abs;
using Walacor_SDK.W_Client.Constants;
using Walacor_SDK.W_Client.Context;
using Walacor_SDK.W_Client.Helpers;

namespace Walacor_SDK.Services.Impl
{
    internal sealed class SchemaService : ISchemaService
    {
        private readonly ClientContext _ctx;
        private readonly string _segment;
        private readonly ILogger _logger;

        public SchemaService(ClientContext ctx, string segment = ApiSegments.Schemas)
        {
            this._ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
            this._segment = string.IsNullOrWhiteSpace(segment) ? ApiSegments.Schemas : segment.Trim('/');
            this._logger = this._ctx.Options.LoggerFactory.CreateLogger<SchemaService>();

            this._logger.LogDebug(
                ServiceLoggingConstants.MsgServiceCreated,
                nameof(SchemaService));
        }

        public async Task<Result<IReadOnlyList<DataTypeDto>>> GetDataTypesAsync(CancellationToken ct = default)
        {
            var sw = Stopwatch.StartNew();

            this._logger.LogDebug(
                ServiceLoggingConstants.MsgMethodEntry,
                nameof(SchemaService),
                nameof(this.GetDataTypesAsync));

            var path = $"{this._segment}/{ApiRoutes.DataTypes}";

            var res = await this._ctx.Transport
                .GetJsonAsync<List<DataTypeDto>>(path, query: null, ct)
                .ConfigureAwait(false);

            var logged = this.LogAndReturn(nameof(this.GetDataTypesAsync), path, res, sw);
            return logged.Map(list => (IReadOnlyList<DataTypeDto>)list.AsReadOnly());
        }

        public async Task<Result<IReadOnlyDictionary<string, AutoGenFieldDto>>> GetPlatformAutoGenerationFieldsAsync(CancellationToken ct = default)
        {
            var sw = Stopwatch.StartNew();

            this._logger.LogDebug(
                ServiceLoggingConstants.MsgMethodEntry,
                nameof(SchemaService),
                nameof(this.GetPlatformAutoGenerationFieldsAsync));

            var path = $"{this._segment}/{ApiRoutes.SystemFields}";

            var res = await this._ctx.Transport
                .GetJsonAsync<Dictionary<string, AutoGenFieldDto>>(path, query: null, ct)
                .ConfigureAwait(false);

            var logged = this.LogAndReturn(nameof(this.GetPlatformAutoGenerationFieldsAsync), path, res, sw);

            return logged.Map(dict =>
                (IReadOnlyDictionary<string, AutoGenFieldDto>)new ReadOnlyDictionary<string, AutoGenFieldDto>(dict));
        }

        public async Task<Result<IReadOnlyList<SchemaEntryDto>>> GetListWithLatestVersionAsync(CancellationToken ct = default)
        {
            var sw = Stopwatch.StartNew();

            this._logger.LogDebug(
                ServiceLoggingConstants.MsgMethodEntry,
                nameof(SchemaService),
                nameof(this.GetListWithLatestVersionAsync));

            var path = $"{this._segment}/{ApiRoutes.VersionsLatest}";

            var res = await this._ctx.Transport
                .GetJsonAsync<List<SchemaEntryDto>>(path, query: null, ct)
                .ConfigureAwait(false);

            var logged = this.LogAndReturn(nameof(this.GetListWithLatestVersionAsync), path, res, sw);
            return logged.Map(list => (IReadOnlyList<SchemaEntryDto>)list.AsReadOnly());
        }

        public async Task<Result<IReadOnlyList<SchemaVersionEntryDto>>> GetVersionsAsync(CancellationToken ct = default)
        {
            var sw = Stopwatch.StartNew();

            this._logger.LogDebug(
                ServiceLoggingConstants.MsgMethodEntry,
                nameof(SchemaService),
                nameof(this.GetVersionsAsync));

            var path = $"{this._segment}/{ApiRoutes.Versions}";

            var res = await this._ctx.Transport
                .GetJsonAsync<List<SchemaVersionEntryDto>>(path, query: null, ct)
                .ConfigureAwait(false);

            var logged = this.LogAndReturn(nameof(this.GetVersionsAsync), path, res, sw);
            return logged.Map(list => (IReadOnlyList<SchemaVersionEntryDto>)list.AsReadOnly());
        }

        public async Task<Result<IReadOnlyList<int>>> GetVersionsForEnvelopeTypeAsync(int etId, CancellationToken ct = default)
        {
            var sw = Stopwatch.StartNew();

            this._logger.LogDebug(
                ServiceLoggingConstants.MsgMethodEntryWithParam,
                nameof(SchemaService),
                nameof(this.GetVersionsForEnvelopeTypeAsync),
                ServiceLoggingConstants.ParamEnvelopeTypeId,
                etId);

            var path = $"{this._segment}/{ApiRoutes.EnvelopeTypes}/{etId}/{ApiRoutes.Versions}";

            var res = await this._ctx.Transport
                .GetJsonAsync<List<int>>(path, query: null, ct)
                .ConfigureAwait(false);

            var logged = this.LogAndReturn(nameof(this.GetVersionsForEnvelopeTypeAsync), path, res, sw);
            return logged.Map(list => (IReadOnlyList<int>)list.AsReadOnly());
        }

        public async Task<Result<IReadOnlyList<IndexEntryDto>>> GetIndexesAsync(int etId, CancellationToken ct = default)
        {
            var sw = Stopwatch.StartNew();

            this._logger.LogDebug(
                ServiceLoggingConstants.MsgMethodEntryWithParam,
                nameof(SchemaService),
                nameof(this.GetIndexesAsync),
                ServiceLoggingConstants.ParamEnvelopeTypeId,
                etId);

            var path = $"{this._segment}/{ApiRoutes.EnvelopeTypes}/{etId}/{ApiRoutes.Indexes}";

            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [HeaderNames.ETId] = etId.ToString(CultureInfo.InvariantCulture),
            };

            var res = await this._ctx.Transport
                .GetJsonWithHeadersAsync<List<IndexEntryDto>>(path, query: null, headers, ct)
                .ConfigureAwait(false);

            var logged = this.LogAndReturn(nameof(this.GetIndexesAsync), path, res, sw);
            return logged.Map(list => (IReadOnlyList<IndexEntryDto>)list.AsReadOnly());
        }

        public async Task<Result<IReadOnlyList<IndexEntryDto>>> GetIndexesAsync(SystemEnvelopeType etId, CancellationToken ct = default)
        {
            var sw = Stopwatch.StartNew();

            this._logger.LogDebug(
                ServiceLoggingConstants.MsgMethodEntryWithParam,
                nameof(SchemaService),
                nameof(this.GetIndexesAsync),
                ServiceLoggingConstants.ParamEnvelopeTypeId,
                etId.ToString());

            var path = $"{this._segment}/{ApiRoutes.EnvelopeTypes}/{etId}/{ApiRoutes.Indexes}";

            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [HeaderNames.ETId] = EnumHelper.EtIdToString(etId),
            };

            var res = await this._ctx.Transport
                .GetJsonWithHeadersAsync<List<IndexEntryDto>>(path, query: null, headers, ct)
                .ConfigureAwait(false);

            var logged = this.LogAndReturn(nameof(this.GetIndexesAsync), path, res, sw);
            return logged.Map(list => (IReadOnlyList<IndexEntryDto>)list.AsReadOnly());
        }

        public async Task<Result<IReadOnlyList<IndexEntryDto>>> GetIndexesByTableNameAsync(string tableName, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            var sw = Stopwatch.StartNew();

            this._logger.LogDebug(
                ServiceLoggingConstants.MsgMethodEntryWithParam,
                nameof(SchemaService),
                nameof(this.GetIndexesByTableNameAsync),
                ServiceLoggingConstants.ParamTableName,
                tableName);

            var path = $"{this._segment}/{ApiRoutes.EnvelopeTypes}/{SystemDefaults.SchemaEnvelopeTypeId}/{ApiRoutes.IndexesByTableName}";

            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [HeaderNames.ETId] = SystemDefaults.SchemaEnvelopeTypeId.ToString(CultureInfo.InvariantCulture),
            };

            var query = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [QueryParamNames.TableName] = tableName,
            };

            var res = await this._ctx.Transport
                .GetJsonWithHeadersAsync<List<IndexEntryDto>>(path, query, headers, ct)
                .ConfigureAwait(false);

            var logged = this.LogAndReturn(nameof(this.GetIndexesByTableNameAsync), path, res, sw);
            return logged.Map(list => (IReadOnlyList<IndexEntryDto>)list.AsReadOnly());
        }

        public async Task<Result<SchemaMetadataDto>> CreateSchemaAsync(CreateSchemaRequest request, CancellationToken ct = default)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var sw = Stopwatch.StartNew();

            this._logger.LogDebug(
                ServiceLoggingConstants.MsgMethodEntry,
                nameof(SchemaService),
                nameof(this.CreateSchemaAsync));

            var path = $"{this._segment}";

            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [HeaderNames.ETId] = SystemDefaults.SchemaEnvelopeTypeId.ToString(CultureInfo.InvariantCulture),
                [HeaderNames.SV] = SystemDefaults.DefaultSchemaVersion.ToString(CultureInfo.InvariantCulture),
            };

            var res = await this._ctx.Transport
                .PostJsonWithHeadersAsync<CreateSchemaRequest, SchemaMetadataDto>(
                    path,
                    request,
                    query: null,
                    headers: headers,
                    ct: ct)
                .ConfigureAwait(false);

            return this.LogAndReturn(nameof(this.CreateSchemaAsync), path, res, sw);
        }

        public async Task<Result<SchemaDetailDto>> GetSchemaDetailsByEnvelopeTypeAsync(int etId, CancellationToken ct = default)
        {
            var sw = Stopwatch.StartNew();

            this._logger.LogDebug(
                ServiceLoggingConstants.MsgMethodEntryWithParam,
                nameof(SchemaService),
                nameof(this.GetSchemaDetailsByEnvelopeTypeAsync),
                ServiceLoggingConstants.ParamEnvelopeTypeId,
                etId);

            var path = $"{this._segment}/{ApiRoutes.EnvelopeTypes}/{etId}/{ApiRoutes.Details}";

            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [HeaderNames.ETidTypo] = etId.ToString(CultureInfo.InvariantCulture),
            };

            var res = await this._ctx.Transport
                .GetJsonWithHeadersAsync<SchemaDetailDto>(path, query: null, headers, ct)
                .ConfigureAwait(false);

            return this.LogAndReturn(nameof(this.GetSchemaDetailsByEnvelopeTypeAsync), path, res, sw);
        }

        public async Task<Result<IReadOnlyList<long>>> GetEnvelopeTypesAsync(CancellationToken ct = default)
        {
            var sw = Stopwatch.StartNew();

            this._logger.LogDebug(
                ServiceLoggingConstants.MsgMethodEntry,
                nameof(SchemaService),
                nameof(this.GetEnvelopeTypesAsync));

            var path = $"{this._segment}/{ApiRoutes.EnvelopeTypes}";

            var res = await this._ctx.Transport
                .GetJsonAsync<List<long>>(path, query: null, ct)
                .ConfigureAwait(false);

            var logged = this.LogAndReturn(nameof(this.GetEnvelopeTypesAsync), path, res, sw);
            return logged.Map(list => (IReadOnlyList<long>)list.AsReadOnly());
        }

        public async Task<Result<SchemaDetailDto>> GetDetailsByIdAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var sw = Stopwatch.StartNew();

            this._logger.LogDebug(
                ServiceLoggingConstants.MsgMethodEntryWithParam,
                nameof(SchemaService),
                nameof(this.GetDetailsByIdAsync),
                "Id",
                id);

            var path = $"{this._segment}/{id}";

            var res = await this._ctx.Transport
                .GetJsonAsync<SchemaDetailDto>(path, query: null, ct)
                .ConfigureAwait(false);

            return this.LogAndReturn(nameof(this.GetDetailsByIdAsync), path, res, sw);
        }

        public async Task<Result<IReadOnlyList<SchemaItemDto>>> GetListSchemaItemsAsync(CancellationToken ct = default)
        {
            var sw = Stopwatch.StartNew();

            this._logger.LogDebug(
                ServiceLoggingConstants.MsgMethodEntry,
                nameof(SchemaService),
                nameof(this.GetListSchemaItemsAsync));

            var path = $"{this._segment}";

            var res = await this._ctx.Transport
                .GetJsonAsync<List<SchemaItemDto>>(path, query: null, ct)
                .ConfigureAwait(false);

            var logged = this.LogAndReturn(nameof(this.GetListSchemaItemsAsync), path, res, sw);
            return logged.Map(list => (IReadOnlyList<SchemaItemDto>)list.AsReadOnly());
        }

        public async Task<Result<Paged<SchemaSummaryDto>>> GetSchemaQuerySchemaItemsAsync(
            SchemaQueryListRequest request,
            CancellationToken ct = default)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var sw = Stopwatch.StartNew();

            this._logger.LogDebug(
                ServiceLoggingConstants.MsgMethodEntry,
                nameof(SchemaService),
                nameof(this.GetSchemaQuerySchemaItemsAsync));

            var path = $"{this._segment}/{ApiRoutes.SchemaList}";
            var query = QueryHelper.BuildQueryFromObject(request);

            var res = await this._ctx.Transport
                .GetJsonAsync<List<SchemaSummaryDto>>(path, query, ct)
                .ConfigureAwait(false);

            sw.Stop();

            if (!res.IsSuccess || res.Value is null)
            {
                this._logger.LogError(
                    ServiceLoggingConstants.MsgMethodFailureWithWire,
                    nameof(SchemaService),
                    nameof(this.GetSchemaQuerySchemaItemsAsync),
                    path,
                    res.StatusCode,
                    res.CorrelationId ?? string.Empty,
                    res.DurationMs ?? sw.ElapsedMilliseconds,
                    res.Error?.Code ?? "UNKNOWN",
                    res.Error?.Message ?? ErrorMessages.RequestFailed);

                return Result<Paged<SchemaSummaryDto>>.Fail(
                    res.Error ?? Error.Unknown(ErrorMessages.RequestFailed),
                    res.StatusCode,
                    res.CorrelationId,
                    res.DurationMs ?? sw.ElapsedMilliseconds);
            }

            var items = (res.Value ?? new List<SchemaSummaryDto>()).AsReadOnly();

            this._logger.LogInformation(
                ServiceLoggingConstants.MsgMethodSuccess,
                nameof(SchemaService),
                nameof(this.GetSchemaQuerySchemaItemsAsync));

            return Result<Paged<SchemaSummaryDto>>.Success(
                new Paged<SchemaSummaryDto>(items, items.Count),
                res.StatusCode,
                res.CorrelationId,
                res.DurationMs ?? sw.ElapsedMilliseconds);
        }

        private Result<T> LogAndReturn<T>(string operation, string path, Result<T> res, Stopwatch sw)
        {
            sw.Stop();

            if (!res.IsSuccess || res.Value is null)
            {
                this._logger.LogError(
                    ServiceLoggingConstants.MsgMethodFailureWithWire,
                    nameof(SchemaService),
                    operation,
                    path,
                    res.StatusCode,
                    res.CorrelationId ?? string.Empty,
                    res.DurationMs ?? sw.ElapsedMilliseconds,
                    res.Error?.Code ?? "UNKNOWN",
                    res.Error?.Message ?? ErrorMessages.RequestFailed);

                return res;
            }

            this._logger.LogInformation(
                ServiceLoggingConstants.MsgMethodSuccess,
                nameof(SchemaService),
                operation);

            return res;
        }
    }
}
