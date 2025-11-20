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
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Walacor_SDK.Models.DataRequests.Response;
using Walacor_SDK.Models.Result;
using Walacor_SDK.Models.Results;
using Walacor_SDK.Services.Abs;
using Walacor_SDK.W_Client.Context;

namespace Walacor_SDK.Services.Impl
{
    internal sealed class DataRequestsService : IDataRequestsService
    {
        private readonly ClientContext _ctx;
        private readonly string _segment;

        public DataRequestsService(ClientContext ctx, string segment = "envelopes")
        {
            this._ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
            this._segment = string.IsNullOrWhiteSpace(segment) ? "envelopes" : segment.Trim('/');
        }

        public async Task<Result<SubmissionResult>> InsertSingleRecordAsync(object jsonRecord, int etId, CancellationToken ct = default)
        {
            var path = $"{this._segment}/submit";

            var body = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["Data"] = new[] { jsonRecord },
            };

            var headers = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["ETId"] = etId.ToString(CultureInfo.InvariantCulture),
            };

            var res = await this._ctx.Transport
                .PostJsonWithHeadersAsync<Dictionary<string, object>, SubmissionResult>(path, body, headers: headers, ct: ct)
                .ConfigureAwait(false);

            return res;
        }

        public async Task<Result<SubmissionResult>> InsertMultipleRecordsAsync(IEnumerable<Dictionary<string, object>> records, int etId, CancellationToken ct = default)
        {
            var path = $"{this._segment}/submit";

            var payload = new Dictionary<string, object>(StringComparer.Ordinal) { ["Data"] = records };
            var headers = new Dictionary<string, string>(StringComparer.Ordinal) { { "ETId", etId.ToString(CultureInfo.InvariantCulture) } };

            var res = await this._ctx.Transport
                .PostJsonWithHeadersAsync<Dictionary<string, object>, SubmissionResult>(path, payload, headers: headers, ct: ct)
                .ConfigureAwait(false);

            return res;
        }

        public async Task<Result<SubmissionResult>> UpdateSingleRecordWithUidAsync(IDictionary<string, object> record, int etId, CancellationToken ct = default)
        {
            var path = $"{this._segment}/submit";

            if (!record.ContainsKey("UID"))
            {
                return Result<SubmissionResult>.Fail(Error.Validation("uid_missing", "UID is required to update a record."));
            }

            var headers = new Dictionary<string, string>(StringComparer.Ordinal) { { "ETId", etId.ToString(CultureInfo.InvariantCulture) } };
            var payload = new Dictionary<string, object>(StringComparer.Ordinal) { ["Data"] = new[] { record } };

            var res = await this._ctx.Transport
                .PostJsonWithHeadersAsync<Dictionary<string, object>, SubmissionResult>(path, payload, headers: headers, ct: ct)
                .ConfigureAwait(false);

            return res;
        }

        public async Task<Result<SubmissionResult>> UpdateMultipleRecordsAsync(IEnumerable<IDictionary<string, object>> records, int etId, CancellationToken ct = default)
        {
            var recordList = records?.ToList() ?? new List<IDictionary<string, object>>();

            if (recordList.Count == 0)
            {
                return Result<SubmissionResult>.Fail(
                    Error.Validation("records_empty", "At least one record is required."));
            }

            foreach (var record in recordList)
            {
                if (!record.ContainsKey("UID"))
                {
                    return Result<SubmissionResult>.Fail(
                        Error.Validation("uid_missing", "All records must contain a UID field."));
                }
            }

            var path = $"{this._segment}/submit";

            var headers = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "ETId", etId.ToString(CultureInfo.InvariantCulture) },
            };

            var payload = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["Data"] = recordList,
            };

            var res = await this._ctx.Transport
                .PostJsonWithHeadersAsync<Dictionary<string, object>, SubmissionResult>(
                    path, payload, headers: headers, ct: ct)
                .ConfigureAwait(false);

            return res;
        }

        public async Task<Result<IReadOnlyList<Dictionary<string, object>>>> GetAllAsync(int etId, int pageNumber = 0, int pageSize = 0, bool fromSummary = true, CancellationToken ct = default)
        {
            var path = "query/get";

            var headers = new Dictionary<string, string>(StringComparer.Ordinal) { { "ETId", etId.ToString(CultureInfo.InvariantCulture) } };

            var query = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["pageNo"] = pageNumber.ToString(CultureInfo.InvariantCulture),
                ["pageSize"] = pageSize.ToString(CultureInfo.InvariantCulture),
                ["fromSummary"] = fromSummary ? "true" : "false",
            };

            var res = await this._ctx.Transport
                .PostJsonWithHeadersAsync<object, List<Dictionary<string, object>>>(path, new { }, query, headers, ct)
                .ConfigureAwait(false);

            return res.Map(list => (IReadOnlyList<Dictionary<string, object>>)list.AsReadOnly());
        }

        public async Task<Result<IReadOnlyList<Dictionary<string, object>>>> GetSingleRecordByIdAsync(string recordId, int etId, bool fromSummary = false, CancellationToken ct = default)
        {
            var path = "query/get";

            var headers = new Dictionary<string, string>(StringComparer.Ordinal) { { "ETId", etId.ToString(CultureInfo.InvariantCulture) } };
            var query = new Dictionary<string, string>(StringComparer.Ordinal) { { "fromSummary", fromSummary ? "true" : "false" } };
            var body = new Dictionary<string, string>(StringComparer.Ordinal) { { "au_id", recordId } };

            var res = await this._ctx.Transport
                .PostJsonWithHeadersAsync<object, List<Dictionary<string, object>>>(path, body, query, headers, ct)
                .ConfigureAwait(false);

            return res.Map(list => (IReadOnlyList<Dictionary<string, object>>)list.AsReadOnly());
        }

        public async Task<Result<IReadOnlyList<Dictionary<string, object>>>> PostComplexQueryAsync(int etId, IReadOnlyList<IReadOnlyDictionary<string, object>> pipeline, CancellationToken ct = default)
        {
            var path = "query/getcomplex";

            var headers = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["ETId"] = etId.ToString(CultureInfo.InvariantCulture),
            };

            var res = await this._ctx.Transport
                .PostJsonWithHeadersAsync<IReadOnlyList<IReadOnlyDictionary<string, object>>, List<Dictionary<string, object>>>(
                    path,
                    pipeline,
                    headers: headers,
                    ct: ct)
                .ConfigureAwait(false);

            return res.Map(list => (IReadOnlyList<Dictionary<string, object>>)list.AsReadOnly());
        }

        public async Task<Result<IReadOnlyList<Dictionary<string, object>>>> PostQueryApiAsync(int etId, IReadOnlyDictionary<string, object> payload, int schemaVersion = 1, int pageNumber = 1, int pageSize = 0, CancellationToken ct = default)
        {
            var path = "query/get";
            var headers = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["ETId"] = etId.ToString(CultureInfo.InvariantCulture),
                ["SV"] = schemaVersion.ToString(CultureInfo.InvariantCulture),
            };

            var query = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["pageNo"] = pageNumber.ToString(CultureInfo.InvariantCulture),
                ["pageSize"] = pageSize.ToString(CultureInfo.InvariantCulture),
            };

            var res = await this._ctx.Transport
                .PostJsonWithHeadersAsync<IReadOnlyDictionary<string, object>, List<Dictionary<string, object>>>(
                    path,
                    payload,
                    query,
                    headers,
                    ct)
                .ConfigureAwait(false);

            return res.Map(list => (IReadOnlyList<Dictionary<string, object>>)list.AsReadOnly());
        }

        public async Task<Result<IReadOnlyList<QueryApiAggregate>>> PostQueryApiAggregateAsync(int etId, IReadOnlyList<IReadOnlyDictionary<string, object>> pipeline, int schemaVersion = 1, int dataVersion = 1, CancellationToken ct = default)
        {
            const string path = "query/getComplex";

            var headers = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["ETId"] = etId.ToString(CultureInfo.InvariantCulture),
                ["SV"] = schemaVersion.ToString(CultureInfo.InvariantCulture),
                ["DV"] = dataVersion.ToString(CultureInfo.InvariantCulture),
            };

            var res = await this._ctx.Transport
                .PostJsonWithHeadersAsync<IReadOnlyList<IReadOnlyDictionary<string, object>>, List<QueryApiAggregate>>(
                    path,
                    pipeline,
                    headers: headers,
                    ct: ct)
                .ConfigureAwait(false);

            return res.Map(list => (IReadOnlyList<QueryApiAggregate>)list.AsReadOnly());
        }

        public async Task<Result<ComplexQMLQueryRecords>> PostComplexMqlQueriesAsync(IEnumerable<IDictionary<string, object>> pipeline, int etId, CancellationToken ct = default)
        {
            const string path = "query/getcomplex";

            var headers = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["ETId"] = etId.ToString(CultureInfo.InvariantCulture),
            };

            var res = await this._ctx.Transport
                .PostJsonWithHeadersAsync<IEnumerable<IDictionary<string, object>>, List<Dictionary<string, object>>>(
                    path,
                    pipeline,
                    headers: headers,
                    ct: ct)
                .ConfigureAwait(false);

            return res.Map(rows =>
            {
                var safeRows = rows ?? new List<Dictionary<string, object>>();

                return new ComplexQMLQueryRecords
                {
                    Records = safeRows,
                    Total = safeRows.Count,
                };
            });
        }
    }
}
