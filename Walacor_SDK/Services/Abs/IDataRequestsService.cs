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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Walacor_SDK.Models.DataRequests.Response;
using Walacor_SDK.Models.Results;

namespace Walacor_SDK.Services.Abs
{
    public interface IDataRequestsService
    {
        Task<Result<SubmissionResult>> InsertSingleRecordAsync(object jsonRecord, int etId, CancellationToken ct = default);

        Task<Result<SubmissionResult>> InsertMultipleRecordsAsync(IEnumerable<Dictionary<string, object>> records, int etId, CancellationToken ct = default);

        Task<Result<SubmissionResult>> UpdateSingleRecordWithUidAsync(IDictionary<string, object> record, int etId, CancellationToken ct = default);

        Task<Result<SubmissionResult>> UpdateMultipleRecordsAsync(IEnumerable<IDictionary<string, object>> records, int etId, CancellationToken ct = default);

        Task<Result<IReadOnlyList<Dictionary<string, object>>>> GetAllAsync(int etId, int pageNumber = 0, int pageSize = 0, bool fromSummary = true, CancellationToken ct = default);

        Task<Result<IReadOnlyList<Dictionary<string, object>>>> GetSingleRecordByIdAsync(string recordId, int etId, bool fromSummary = false, CancellationToken ct = default);

        Task<Result<IReadOnlyList<Dictionary<string, object>>>> PostComplexQueryAsync(int etId, IReadOnlyList<IReadOnlyDictionary<string, object>> pipeline, CancellationToken ct = default);

        Task<Result<IReadOnlyList<Dictionary<string, object>>>> PostQueryApiAsync(int etId, IReadOnlyDictionary<string, object> payload, int schemaVersion = 1, int pageNumber = 1, int pageSize = 0, CancellationToken ct = default);

        Task<Result<IReadOnlyList<QueryApiAggregate>>> PostQueryApiAggregateAsync(int etId, IReadOnlyList<IReadOnlyDictionary<string, object>> pipeline, int schemaVersion = 1, int dataVersion = 1, CancellationToken ct = default);

        Task<Result<ComplexQMLQueryRecords>> PostComplexMqlQueriesAsync(IEnumerable<IDictionary<string, object>> pipeline, int etId, CancellationToken ct = default);
    }
}
