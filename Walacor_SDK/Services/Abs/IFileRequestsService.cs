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
using Walacor_SDK.Models.FileRequests.Request;
using Walacor_SDK.Models.FileRequests.Response;
using Walacor_SDK.Models.Results;

namespace Walacor_SDK.Services.Abs
{
    public interface IFileRequestsService
    {
        Task<Result<FileVerificationResult>> VerifyAsync(VerifySingleFileRequest request, CancellationToken ct = default);

        Task<Result<StoreFileData>> StoreAsync(FileInfo fileInfo, CancellationToken ct = default);

        // Task<Result<string>> DownloadAsync(string uid, string? saveTo = null, CancellationToken ct = default);

        // Task<Result<IReadOnlyList<FileMetadata>>> ListFilesAsync(string? uid = null, int pageSize = 0, int pageNo = 0, bool fromSummary = false, bool requestTotal = true, CancellationToken ct = default);
    }
}
