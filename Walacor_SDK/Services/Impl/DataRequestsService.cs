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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Walacor_SDK.Models.DataRequests.Response;
using Walacor_SDK.Models.Result;
using Walacor_SDK.Models.Results;
using Walacor_SDK.Services.Abs;
using Walacor_SDK.W_Client.Constants;
using Walacor_SDK.W_Client.Context;

namespace Walacor_SDK.Services.Impl
{
    internal sealed class DataRequestsService : IDataRequestsService
    {
        private const string LocalPathMarker = "local";

        private readonly ClientContext _ctx;
        private readonly string _segment;
        private readonly ILogger _logger;

        public DataRequestsService(ClientContext ctx, string segment = ApiSegments.Envelopes)
        {
            this._ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
            this._segment = string.IsNullOrWhiteSpace(segment) ? ApiSegments.Envelopes : segment.Trim('/');
            this._logger = this._ctx.Options.LoggerFactory.CreateLogger<DataRequestsService>();

            this._logger.LogDebug(
                ServiceLoggingConstants.MsgServiceCreated,
                nameof(DataRequestsService));
        }

        public async Task<Result<SubmissionResult>> InsertSingleRecordAsync(object jsonRecord, int etId, CancellationToken ct = default)
        {
            if (jsonRecord is null)
            {
                throw new ArgumentNullException(nameof(jsonRecord));
            }

            var sw = Stopwatch.StartNew();

            this._logger.LogDebug(
                ServiceLoggingConstants.MsgMethodEntryWithParam,
                nameof(DataRequestsService),
                nameof(this.InsertSingleRecordAsync),
                ServiceLoggingConstants.ParamEnvelopeTypeId,
                etId);

            var path = $"{this._segment}/{ApiRoutes.Submit}";

            var body = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                [JsonFieldNames.Data] = new[] { jsonRecord },
            };

            var headers = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [HeaderNames.ETId] = etId.ToString(CultureInfo.InvariantCulture),
            };

            var res = await this._ctx.Transport
                .PostJsonWithHeadersAsync<Dictionary<string, object>, SubmissionResult>(
                    path,
                    body,
                    headers: headers,
                    ct: ct)
                .ConfigureAwait(false);

            return this.LogAndReturn(nameof(this.InsertSingleRecordAsync), path, res, sw);
        }

        public async Task<Result<SubmissionResult>> InsertMultipleRecordsAsync(IEnumerable<Dictionary<string, object>> records, int etId, CancellationToken ct = default)
        {
            if (records is null)
            {
                throw new ArgumentNullException(nameof(records));
            }

            var sw = Stopwatch.StartNew();

            var recordList = records.ToList();

            this._logger.LogDebug(
                ServiceLoggingConstants.MsgMethodEntryWithParams,
                nameof(DataRequestsService),
                nameof(this.InsertMultipleRecordsAsync),
                ServiceLoggingConstants.ParamEnvelopeTypeId,
                etId,
                ServiceLoggingConstants.ParamRecordsCount,
                recordList.Count);

            var path = $"{this._segment}/{ApiRoutes.Submit}";

            var payload = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                [JsonFieldNames.Data] = recordList,
            };

            var headers = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [HeaderNames.ETId] = etId.ToString(CultureInfo.InvariantCulture),
            };

            var res = await this._ctx.Transport
                .PostJsonWithHeadersAsync<Dictionary<string, object>, SubmissionResult>(
                    path,
                    payload,
                    headers: headers,
                    ct: ct)
                .ConfigureAwait(false);

            return this.LogAndReturn(nameof(this.InsertMultipleRecordsAsync), path, res, sw);
        }

        public async Task<Result<SubmissionResult>> UpdateSingleRecordWithUidAsync(IDictionary<string, object> record, int etId, CancellationToken ct = default)
        {
            if (record is null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            var sw = Stopwatch.StartNew();

            this._logger.LogDebug(
                ServiceLoggingConstants.MsgMethodEntryWithParam,
                nameof(DataRequestsService),
                nameof(this.UpdateSingleRecordWithUidAsync),
                ServiceLoggingConstants.ParamEnvelopeTypeId,
                etId);

            var path = $"{this._segment}/{ApiRoutes.Submit}";

            if (!record.ContainsKey(JsonFieldNames.UID))
            {
                sw.Stop();

                this._logger.LogWarning(
                    ServiceLoggingConstants.MsgMethodFailureWithWire,
                    nameof(DataRequestsService),
                    nameof(this.UpdateSingleRecordWithUidAsync),
                    LocalPathMarker,
                    null,
                    string.Empty,
                    sw.ElapsedMilliseconds,
                    ErrorCodes.UidMissing,
                    ErrorMessages.UidRequiredToUpdate);

                return Result<SubmissionResult>.Fail(
                    Error.Validation(ErrorCodes.UidMissing, ErrorMessages.UidRequiredToUpdate),
                    null,
                    null,
                    sw.ElapsedMilliseconds);
            }

            var headers = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [HeaderNames.ETId] = etId.ToString(CultureInfo.InvariantCulture),
            };

            var payload = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                [JsonFieldNames.Data] = new[] { record },
            };

            var res = await this._ctx.Transport
                .PostJsonWithHeadersAsync<Dictionary<string, object>, SubmissionResult>(
                    path,
                    payload,
                    headers: headers,
                    ct: ct)
                .ConfigureAwait(false);

            return this.LogAndReturn(nameof(this.UpdateSingleRecordWithUidAsync), path, res, sw);
        }

#pragma warning disable MA0051 // Method is too long
        public async Task<Result<SubmissionResult>> UpdateMultipleRecordsAsync(IEnumerable<IDictionary<string, object>> records, int etId, CancellationToken ct = default)
#pragma warning restore MA0051 // Method is too long
        {
            if (records is null)
            {
                throw new ArgumentNullException(nameof(records));
            }

            var sw = Stopwatch.StartNew();

            var recordList = records.ToList();

            this._logger.LogDebug(
                ServiceLoggingConstants.MsgMethodEntryWithParams,
                nameof(DataRequestsService),
                nameof(this.UpdateMultipleRecordsAsync),
                ServiceLoggingConstants.ParamEnvelopeTypeId,
                etId,
                ServiceLoggingConstants.ParamRecordsCount,
                recordList.Count);

            if (recordList.Count == 0)
            {
                sw.Stop();

                this._logger.LogWarning(
                    ServiceLoggingConstants.MsgMethodFailureWithWire,
                    nameof(DataRequestsService),
                    nameof(this.UpdateMultipleRecordsAsync),
                    LocalPathMarker,
                    null,
                    string.Empty,
                    sw.ElapsedMilliseconds,
                    ErrorCodes.RecordsEmpty,
                    ErrorMessages.RecordsAtLeastOneRequired);

                return Result<SubmissionResult>.Fail(
                    Error.Validation(ErrorCodes.RecordsEmpty, ErrorMessages.RecordsAtLeastOneRequired),
                    null,
                    null,
                    sw.ElapsedMilliseconds);
            }

            foreach (var record in recordList)
            {
                if (!record.ContainsKey(JsonFieldNames.UID))
                {
                    sw.Stop();

                    this._logger.LogWarning(
                        ServiceLoggingConstants.MsgMethodFailureWithWire,
                        nameof(DataRequestsService),
                        nameof(this.UpdateMultipleRecordsAsync),
                        LocalPathMarker,
                        null,
                        string.Empty,
                        sw.ElapsedMilliseconds,
                        ErrorCodes.UidMissing,
                        ErrorMessages.AllRecordsMustContainUid);

                    return Result<SubmissionResult>.Fail(
                        Error.Validation(ErrorCodes.UidMissing, ErrorMessages.AllRecordsMustContainUid),
                        null,
                        null,
                        sw.ElapsedMilliseconds);
                }
            }

            var path = $"{this._segment}/{ApiRoutes.Submit}";

            var headers = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [HeaderNames.ETId] = etId.ToString(CultureInfo.InvariantCulture),
            };

            var payload = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                [JsonFieldNames.Data] = recordList,
            };

            var res = await this._ctx.Transport
                .PostJsonWithHeadersAsync<Dictionary<string, object>, SubmissionResult>(
                    path,
                    payload,
                    headers: headers,
                    ct: ct)
                .ConfigureAwait(false);

            return this.LogAndReturn(nameof(this.UpdateMultipleRecordsAsync), path, res, sw);
        }

        public async Task<Result<IReadOnlyList<Dictionary<string, object>>>> GetAllAsync(
            int etId,
            int pageNumber = 0,
            int pageSize = 0,
            bool fromSummary = true,
            CancellationToken ct = default)
        {
            var sw = Stopwatch.StartNew();

            this._logger.LogDebug(
                ServiceLoggingConstants.MsgMethodEntryWithParams4,
                nameof(DataRequestsService),
                nameof(this.GetAllAsync),
                ServiceLoggingConstants.ParamEnvelopeTypeId,
                etId,
                ServiceLoggingConstants.ParamPageNumber,
                pageNumber,
                ServiceLoggingConstants.ParamPageSize,
                pageSize,
                ServiceLoggingConstants.ParamFromSummary,
                fromSummary);

            var path = ApiRoutes.QueryGet;

            var headers = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [HeaderNames.ETId] = etId.ToString(CultureInfo.InvariantCulture),
            };

            var query = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [QueryParamNames.PageNo] = pageNumber.ToString(CultureInfo.InvariantCulture),
                [QueryParamNames.PageSize] = pageSize.ToString(CultureInfo.InvariantCulture),
                [QueryParamNames.FromSummary] = fromSummary ? BooleanStrings.True : BooleanStrings.False,
            };

            var res = await this._ctx.Transport
                .PostJsonWithHeadersAsync<object, List<Dictionary<string, object>>>(
                    path,
                    new { },
                    query,
                    headers,
                    ct)
                .ConfigureAwait(false);

            // Log based on wire result, then map.
            var logged = this.LogAndReturn(nameof(this.GetAllAsync), path, res, sw);
            return logged.Map(list => (IReadOnlyList<Dictionary<string, object>>)list.AsReadOnly());
        }

        public async Task<Result<IReadOnlyList<Dictionary<string, object>>>> GetSingleRecordByIdAsync(
            string recordId,
            int etId,
            bool fromSummary = false,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(recordId))
            {
                throw new ArgumentNullException(nameof(recordId));
            }

            var sw = Stopwatch.StartNew();

            this._logger.LogDebug(
                ServiceLoggingConstants.MsgMethodEntryWithParams3,
                nameof(DataRequestsService),
                nameof(this.GetSingleRecordByIdAsync),
                ServiceLoggingConstants.ParamRecordId,
                recordId,
                ServiceLoggingConstants.ParamEnvelopeTypeId,
                etId,
                ServiceLoggingConstants.ParamFromSummary,
                fromSummary);

            var path = ApiRoutes.QueryGet;

            var headers = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [HeaderNames.ETId] = etId.ToString(CultureInfo.InvariantCulture),
            };

            var query = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [QueryParamNames.FromSummary] = fromSummary ? BooleanStrings.True : BooleanStrings.False,
            };

            var body = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [JsonFieldNames.AuId] = recordId,
            };

            var res = await this._ctx.Transport
                .PostJsonWithHeadersAsync<object, List<Dictionary<string, object>>>(
                    path,
                    body,
                    query,
                    headers,
                    ct)
                .ConfigureAwait(false);

            var logged = this.LogAndReturn(nameof(this.GetSingleRecordByIdAsync), path, res, sw);
            return logged.Map(list => (IReadOnlyList<Dictionary<string, object>>)list.AsReadOnly());
        }

        public async Task<Result<IReadOnlyList<Dictionary<string, object>>>> PostComplexQueryAsync(
            int etId,
            IReadOnlyList<IReadOnlyDictionary<string, object>> pipeline,
            CancellationToken ct = default)
        {
            if (pipeline is null)
            {
                throw new ArgumentNullException(nameof(pipeline));
            }

            var sw = Stopwatch.StartNew();

            this._logger.LogDebug(
                ServiceLoggingConstants.MsgMethodEntryWithParams,
                nameof(DataRequestsService),
                nameof(this.PostComplexQueryAsync),
                ServiceLoggingConstants.ParamEnvelopeTypeId,
                etId,
                ServiceLoggingConstants.ParamPipelineCount,
                pipeline.Count);

            var path = ApiRoutes.QueryGetComplexLower;

            var headers = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [HeaderNames.ETId] = etId.ToString(CultureInfo.InvariantCulture),
            };

            var res = await this._ctx.Transport
                .PostJsonWithHeadersAsync<IReadOnlyList<IReadOnlyDictionary<string, object>>, List<Dictionary<string, object>>>(
                    path,
                    pipeline,
                    headers: headers,
                    ct: ct)
                .ConfigureAwait(false);

            var logged = this.LogAndReturn(nameof(this.PostComplexQueryAsync), path, res, sw);
            return logged.Map(list => (IReadOnlyList<Dictionary<string, object>>)list.AsReadOnly());
        }

        public async Task<Result<IReadOnlyList<Dictionary<string, object>>>> PostQueryApiAsync(
            int etId,
            IReadOnlyDictionary<string, object> payload,
            int schemaVersion = 1,
            int pageNumber = 1,
            int pageSize = 0,
            CancellationToken ct = default)
        {
            if (payload is null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            var sw = Stopwatch.StartNew();

            this._logger.LogDebug(
                ServiceLoggingConstants.MsgMethodEntryWithParams5,
                nameof(DataRequestsService),
                nameof(this.PostQueryApiAsync),
                ServiceLoggingConstants.ParamEnvelopeTypeId,
                etId,
                ServiceLoggingConstants.ParamSchemaVersion,
                schemaVersion,
                ServiceLoggingConstants.ParamPageNumber,
                pageNumber,
                ServiceLoggingConstants.ParamPageSize,
                pageSize,
                ServiceLoggingConstants.ParamRecordsCount,
                payload.Count);

            var path = ApiRoutes.QueryGet;

            var headers = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [HeaderNames.ETId] = etId.ToString(CultureInfo.InvariantCulture),
                [HeaderNames.SV] = schemaVersion.ToString(CultureInfo.InvariantCulture),
            };

            var query = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [QueryParamNames.PageNo] = pageNumber.ToString(CultureInfo.InvariantCulture),
                [QueryParamNames.PageSize] = pageSize.ToString(CultureInfo.InvariantCulture),
            };

            var res = await this._ctx.Transport
                .PostJsonWithHeadersAsync<IReadOnlyDictionary<string, object>, List<Dictionary<string, object>>>(
                    path,
                    payload,
                    query,
                    headers,
                    ct)
                .ConfigureAwait(false);

            var logged = this.LogAndReturn(nameof(this.PostQueryApiAsync), path, res, sw);
            return logged.Map(list => (IReadOnlyList<Dictionary<string, object>>)list.AsReadOnly());
        }

        public async Task<Result<IReadOnlyList<QueryApiAggregate>>> PostQueryApiAggregateAsync(
            int etId,
            IReadOnlyList<IReadOnlyDictionary<string, object>> pipeline,
            int schemaVersion = 1,
            int dataVersion = 1,
            CancellationToken ct = default)
        {
            if (pipeline is null)
            {
                throw new ArgumentNullException(nameof(pipeline));
            }

            var sw = Stopwatch.StartNew();

            this._logger.LogDebug(
                ServiceLoggingConstants.MsgMethodEntryWithParams4,
                nameof(DataRequestsService),
                nameof(this.PostQueryApiAggregateAsync),
                ServiceLoggingConstants.ParamEnvelopeTypeId,
                etId,
                ServiceLoggingConstants.ParamSchemaVersion,
                schemaVersion,
                ServiceLoggingConstants.ParamDataVersion,
                dataVersion,
                ServiceLoggingConstants.ParamPipelineCount,
                pipeline.Count);

            const string path = ApiRoutes.QueryGetComplexCamel;

            var headers = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [HeaderNames.ETId] = etId.ToString(CultureInfo.InvariantCulture),
                [HeaderNames.SV] = schemaVersion.ToString(CultureInfo.InvariantCulture),
                [HeaderNames.DV] = dataVersion.ToString(CultureInfo.InvariantCulture),
            };

            var res = await this._ctx.Transport
                .PostJsonWithHeadersAsync<IReadOnlyList<IReadOnlyDictionary<string, object>>, List<QueryApiAggregate>>(
                    path,
                    pipeline,
                    headers: headers,
                    ct: ct)
                .ConfigureAwait(false);

            var logged = this.LogAndReturn(nameof(this.PostQueryApiAggregateAsync), path, res, sw);
            return logged.Map(list => (IReadOnlyList<QueryApiAggregate>)list.AsReadOnly());
        }

        public async Task<Result<ComplexQMLQueryRecords>> PostComplexMqlQueriesAsync(
            IEnumerable<IDictionary<string, object>> pipeline,
            int etId,
            CancellationToken ct = default)
        {
            if (pipeline is null)
            {
                throw new ArgumentNullException(nameof(pipeline));
            }

            var sw = Stopwatch.StartNew();

            var pipelineList = pipeline.ToList();

            this._logger.LogDebug(
                ServiceLoggingConstants.MsgMethodEntryWithParams,
                nameof(DataRequestsService),
                nameof(this.PostComplexMqlQueriesAsync),
                ServiceLoggingConstants.ParamEnvelopeTypeId,
                etId,
                ServiceLoggingConstants.ParamPipelineCount,
                pipelineList.Count);

            const string path = ApiRoutes.QueryGetComplexLower;

            var headers = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [HeaderNames.ETId] = etId.ToString(CultureInfo.InvariantCulture),
            };

            var res = await this._ctx.Transport
                .PostJsonWithHeadersAsync<IEnumerable<IDictionary<string, object>>, List<Dictionary<string, object>>>(
                    path,
                    pipelineList,
                    headers: headers,
                    ct: ct)
                .ConfigureAwait(false);

            var logged = this.LogAndReturn(nameof(this.PostComplexMqlQueriesAsync), path, res, sw);

            return logged.Map(rows =>
            {
                var safeRows = rows ?? new List<Dictionary<string, object>>();

                return new ComplexQMLQueryRecords
                {
                    Records = safeRows,
                    Total = safeRows.Count,
                };
            });
        }

        private Result<T> LogAndReturn<T>(string operation, string path, Result<T> res, Stopwatch sw)
        {
            sw.Stop();

            if (!res.IsSuccess || res.Value is null)
            {
                this._logger.LogError(
                    ServiceLoggingConstants.MsgMethodFailureWithWire,
                    nameof(DataRequestsService),
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
                nameof(DataRequestsService),
                operation);

            return res;
        }
    }
}
