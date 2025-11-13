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
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
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

        public DataRequestsService(ClientContext ctx, string segment = "dataRequests")
        {
            this._ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
            this._segment = string.IsNullOrWhiteSpace(segment) ? "dataRequests" : segment.Trim('/');
        }

        public async Task<Result<SubmissionResult>> InsertSingleRecordAsync(string jsonRecord, int etId, CancellationToken ct = default)
        {
            var record = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["Data"] = new[] { jsonRecord },
            };
            var headers = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["ETId"] = etId.ToString(CultureInfo.InvariantCulture),
            };

            var res = await this._ctx.Transport
                .PostJsonWithHeadersAsync<Dictionary<string, object>, SubmissionResult>("envelopes/submit", record, headers: headers, ct: ct)
                .ConfigureAwait(false);

            return res;
        }

        public async Task<Result<SubmissionResult>> InsertMultipleRecordsAsync(IEnumerable<Dictionary<string, object>> records, int etId, CancellationToken ct = default)
        {
            var payload = new Dictionary<string, object>(StringComparer.Ordinal) { ["Data"] = records };
            var headers = new Dictionary<string, string>(StringComparer.Ordinal) { { "ETId", etId.ToString(CultureInfo.InvariantCulture) } };

            var res = await this._ctx.Transport
                .PostJsonWithHeadersAsync<Dictionary<string, object>, SubmissionResult>("envelopes/submit", payload, headers: headers, ct: ct)
                .ConfigureAwait(false);

            return res;
        }

        public async Task<Result<SubmissionResult>> UpdateSingleRecordWithUidAsync(Dictionary<string, object> record, int etId, CancellationToken ct = default)
        {
            if (!record.ContainsKey("UID"))
            {
                return Result<SubmissionResult>.Fail(Error.Validation("uid_missing", "UID is required to update a record."));
            }

            var headers = new Dictionary<string, string>(StringComparer.Ordinal) { { "ETId", etId.ToString(CultureInfo.InvariantCulture) } };
            var payload = new Dictionary<string, object>(StringComparer.Ordinal) { ["Data"] = new[] { record } };

            var res = await this._ctx.Transport
                .PostJsonWithHeadersAsync<Dictionary<string, object>, SubmissionResult>("envelopes/submit", payload, headers: headers, ct: ct)
                .ConfigureAwait(false);

            return res;
        }

        public async Task<Result<SubmissionResult>> UpdateMultipleRecordsAsync(IEnumerable<string> records, int etId, CancellationToken ct = default)
        {
            try
            {
                foreach (var record in records)
                {
                    var parsed = JsonConvert.DeserializeObject<Dictionary<string, object>>(record);
                    if (parsed is null || !parsed.ContainsKey("UID"))
                    {
                        return Result<SubmissionResult>.Fail(Error.Validation("uid_missing", "All records must contain a UID field."));
                    }
                }
            }
            catch (JsonException e)
            {
                return Result<SubmissionResult>.Fail(Error.Validation("invalid_json", $"Invalid JSON in records: {e.Message}"));
            }

            var headers = new Dictionary<string, string>(StringComparer.Ordinal) { { "ETId", etId.ToString(CultureInfo.InvariantCulture) } };
            var payload = new Dictionary<string, object>(StringComparer.Ordinal) { ["Data"] = records };

            var res = await this._ctx.Transport
                .PostJsonWithHeadersAsync<Dictionary<string, object>, SubmissionResult>("envelopes/submit", payload, headers: headers, ct: ct)
                .ConfigureAwait(false);

            return res;
        }

        // ------------------------------------------------------------------ READ – simple
        public async Task<Result<IReadOnlyList<Dictionary<string, object>>>> GetAllAsync(int etId, int pageNumber = 0, int pageSize = 0, bool fromSummary = false, CancellationToken ct = default)
        {
            var headers = new Dictionary<string, string>(StringComparer.Ordinal) { { "ETId", etId.ToString(CultureInfo.InvariantCulture) } };

            var query = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["pageNo"] = pageNumber.ToString(CultureInfo.InvariantCulture),
                ["pageSize"] = pageSize.ToString(CultureInfo.InvariantCulture),
                ["fromSummary"] = fromSummary ? "true" : "false",
            };

            var res = await this._ctx.Transport
                .PostJsonWithHeadersAsync<object, List<Dictionary<string, object>>>("query/get", new { }, query, headers, ct)
                .ConfigureAwait(false);

            return res.Map(list => (IReadOnlyList<Dictionary<string, object>>)list.AsReadOnly());
        }

        public async Task<Result<IReadOnlyList<Dictionary<string, object>>>> GetSingleRecordByIdAsync(Dictionary<string, string> recordId, int etId, bool fromSummary = false, CancellationToken ct = default)
        {
            var headers = new Dictionary<string, string>(StringComparer.Ordinal) { { "ETId", etId.ToString(CultureInfo.InvariantCulture) } };
            var query = new Dictionary<string, string>(StringComparer.Ordinal) { { "fromSummary", fromSummary ? "true" : "false" } };

            var res = await this._ctx.Transport
                .PostJsonWithHeadersAsync<Dictionary<string, string>, List<Dictionary<string, object>>>("query/get", recordId, query, headers, ct)
                .ConfigureAwait(false);

            return res.Map(list => (IReadOnlyList<Dictionary<string, object>>)list.AsReadOnly());
        }

        // ------------------------------------------------------------------ READ – complex / aggregate
        public async Task<Result<ComplexQueryRecords>> PostComplexQueryAsync(int etId, IEnumerable<Dictionary<string, object>> pipeline, CancellationToken ct = default)
        {
            var headers = new Dictionary<string, string>(StringComparer.Ordinal) { { "ETId", etId.ToString(CultureInfo.InvariantCulture) } };

            var res = await this._ctx.Transport
                .PostJsonWithHeadersAsync<IEnumerable<Dictionary<string, object>>, ComplexQueryRecords>("query/getcomplex", pipeline, headers: headers, ct: ct)
                .ConfigureAwait(false);

            return res;
        }

        public async Task<Result<IReadOnlyList<string>>> PostQueryApiAsync(int etId, Dictionary<string, object> payload, int schemaVersion = 1, int pageNumber = 1, int pageSize = 0, CancellationToken ct = default)
        {
            var headers = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "ETId", etId.ToString(CultureInfo.InvariantCulture) },
                { "SV", schemaVersion.ToString(CultureInfo.InvariantCulture) },
            };

            var query = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["pageNo"] = pageNumber.ToString(CultureInfo.InvariantCulture),
                ["pageSize"] = pageSize.ToString(CultureInfo.InvariantCulture),
            };

            var res = await this._ctx.Transport
                .PostJsonWithHeadersAsync<Dictionary<string, object>, List<string>>("query/get", payload, query, headers, ct)
                .ConfigureAwait(false);

            return res.Map(list => (IReadOnlyList<string>)list.AsReadOnly());
        }

        public async Task<Result<QueryApiAggregate>> PostQueryApiAggregateAsync(IEnumerable<Dictionary<string, object>> payload, int etId = 10, int schemaVersion = 1, int dataVersion = 1, CancellationToken ct = default)
        {
            var headers = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "ETId", etId.ToString(CultureInfo.InvariantCulture) },
                { "SV", schemaVersion.ToString(CultureInfo.InvariantCulture) },
                { "DV", dataVersion.ToString(CultureInfo.InvariantCulture) },
            };

            var res = await this._ctx.Transport
                .PostJsonWithHeadersAsync<IEnumerable<Dictionary<string, object>>, QueryApiAggregate>("query/getComplex", payload, headers: headers, ct: ct)
                .ConfigureAwait(false);

            return res;
        }

        public async Task<Result<ComplexQMLQueryRecords>> PostComplexMqlQueriesAsync(IEnumerable<Dictionary<string, object>> pipeline, int etId, CancellationToken ct = default)
        {
            var headers = new Dictionary<string, string>(StringComparer.Ordinal) { { "ETId", etId.ToString(CultureInfo.InvariantCulture) } };

            var res = await this._ctx.Transport
                .PostJsonWithHeadersAsync<IEnumerable<Dictionary<string, object>>, ComplexQMLQueryRecords>("query/getcomplex", pipeline, headers: headers, ct: ct)
                .ConfigureAwait(false);

            return res;
        }
    }
}
