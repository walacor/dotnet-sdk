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
using Walacor_SDK.Enums;
using Walacor_SDK.Models.Results;
using Walacor_SDK.Models.Schema.Request;
using Walacor_SDK.Models.Schema.Response;

namespace Walacor_SDK.Services.Abs
{
    public interface ISchemaService
    {
        Task<Result<IReadOnlyList<DataTypeDto>>> GetDataTypesAsync(CancellationToken ct = default);

        Task<Result<IReadOnlyDictionary<string, AutoGenFieldDto>>> GetPlatformAutoGenerationFieldsAsync(CancellationToken ct = default);

        Task<Result<IReadOnlyList<SchemaEntryDto>>> GetListWithLatestVersionAsync(CancellationToken ct = default);

        Task<Result<IReadOnlyList<SchemaVersionEntryDto>>> GetVersionsAsync(CancellationToken ct = default);

        Task<Result<IReadOnlyList<int>>> GetVersionsForEnvelopeTypeAsync(int etId, CancellationToken ct = default);

        Task<Result<IReadOnlyList<IndexEntryDto>>> GetIndexesAsync(int etId, CancellationToken ct = default);

        Task<Result<IReadOnlyList<IndexEntryDto>>> GetIndexesAsync(SystemEnvelopeType etId, CancellationToken ct = default);

        Task<Result<IReadOnlyList<IndexEntryDto>>> GetIndexesByTableNameAsync(string tableName, CancellationToken ct = default);

        Task<Result<SchemaMetadataDto>> CreateSchemaAsync(CreateSchemaRequest request, CancellationToken ct = default);

        Task<Result<SchemaDetailDto>> GetSchemaDetailsByEnvelopeTypeAsync(int etId, CancellationToken ct = default);

        Task<Result<IReadOnlyList<long>>> GetEnvelopeTypesAsync(CancellationToken ct = default);

        Task<Result<SchemaDetailDto>> GetDetailsByIdAsync(string id, CancellationToken ct = default);

        Task<Result<IReadOnlyList<SchemaItemDto>>> GetListSchemaItemsAsync(CancellationToken ct = default);

        Task<Result<Paged<SchemaSummaryDto>>> GetSchemaQuerySchemaItemsAsync(SchemaQueryListRequest request, CancellationToken ct = default);
    }
}
