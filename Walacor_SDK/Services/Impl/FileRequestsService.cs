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
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Walacor_SDK.Models.FileRequests.Request;
using Walacor_SDK.Models.FileRequests.Response;
using Walacor_SDK.Models.Result;
using Walacor_SDK.Models.Results;
using Walacor_SDK.Services.Abs;
using Walacor_SDK.W_Client.Context;
using Walacor_SDK.W_Client.Helpers;
using FileInfo = Walacor_SDK.Models.FileRequests.Response.FileInfo;

namespace Walacor_SDK.Services.Impl
{
    internal class FileRequestsService : IFileRequestsService
    {
        private readonly ClientContext _ctx;
        private readonly string _segment;

        public FileRequestsService(ClientContext ctx, string segment = "v2/files")
        {
            this._ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
            this._segment = string.IsNullOrWhiteSpace(segment) ? "v2/files" : segment.Trim('/');
        }

        public async Task<Result<FileVerificationResult>> VerifyAsync(
            VerifySingleFileRequest request,
            CancellationToken ct = default)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var fullPath = Path.GetFullPath(request.Path);

            if (!File.Exists(fullPath))
            {
                return Result<FileVerificationResult>.Fail(
                    Error.Validation("file_not_found", "The file to verify does not exist."),
                    null,
                    null);
            }

            var fileName = request.FileName;
            var mimeType = request.MimeType
                            ?? MimeTypeHelper.GetMimeType(fileName, "application/octet-stream");

            using var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);

            using var multipart = new MultipartFormDataContent();

            multipart.Add(fileContent, "file", fileName);

            var path = string.Concat(this._segment, "/verify");

            var wireResult = await this._ctx.Transport
                .PostMultipartAsync<VerifyResponseDto>(path, multipart, ct)
                .ConfigureAwait(false);

            if (!wireResult.IsSuccess || wireResult.Value is null)
            {
                return Result<FileVerificationResult>.Fail(
                    wireResult.Error ?? Error.Unknown("Verification failed."),
                    wireResult.StatusCode,
                    wireResult.CorrelationId);
            }

            var dto = wireResult.Value;

            if (dto.FileInfo is null)
            {
                return Result<FileVerificationResult>.Fail(
                    Error.Deserialization("Verify response did not contain 'fileInfo'."),
                    wireResult.StatusCode,
                    wireResult.CorrelationId);
            }

            var verification = FileVerificationResult.FromFileInfo(dto.FileInfo);

            return Result<FileVerificationResult>.Success(
                verification,
                wireResult.StatusCode,
                wireResult.CorrelationId);
        }

        public async Task<Result<StoreFileData>> StoreAsync(FileInfo fileInfo, CancellationToken ct = default)
        {
            if (fileInfo is null)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }

            var path = $"{this._segment}/store";

            var payload = new StoreFileRequest(fileInfo);

            var res = await this._ctx.Transport
                .PostJsonAsync<StoreFileRequest, StoreFileData>(path, payload, ct)
                .ConfigureAwait(false);

            return res;
        }
    }
}
