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
using System.IO;
using System.Linq;
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

#pragma warning disable MA0051 // Method is too long
        public async Task<Result<string>> DownloadAsync(string uid, string? saveTo = null, CancellationToken ct = default)
#pragma warning restore MA0051 // Method is too long
        {
            if (string.IsNullOrWhiteSpace(uid))
            {
                throw new ArgumentNullException(nameof(uid));
            }

            var listResult = await this.ListFilesAsync(uid: uid, ct: ct).ConfigureAwait(false);

            if (!listResult.IsSuccess || listResult.Value is null || listResult.Value.Count == 0)
            {
                return Result<string>.Fail(
                    listResult.Error ?? Error.NotFound("File not found."),
                    listResult.StatusCode,
                    listResult.CorrelationId,
                    listResult.DurationMs);
            }

            FileMetadata? meta = listResult.Value.FirstOrDefault();
            if (meta is null)
            {
                return Result<string>.Fail(
                    Error.NotFound("File not found."),
                    listResult.StatusCode,
                    listResult.CorrelationId,
                    listResult.DurationMs);
            }

            if (string.Equals(meta.Status, "stored", StringComparison.OrdinalIgnoreCase) == false)
            {
                return Result<string>.Fail(
                    Error.Validation("file_not_ready", $"File is not ready to download. Status: {meta.Status ?? "unknown"}"),
                    listResult.StatusCode,
                    listResult.CorrelationId,
                    listResult.DurationMs);
            }

            if (meta.IsDeleted == true)
            {
                return Result<string>.Fail(
                    Error.NotFound("File was deleted."),
                    listResult.StatusCode,
                    listResult.CorrelationId,
                    listResult.DurationMs);
            }

            string preferredFileName;

            var nameFromServer = meta.Name;
            var mimeFromServer = meta.MimeType;

            if (!string.IsNullOrWhiteSpace(nameFromServer))
            {
                preferredFileName = nameFromServer;

                if (string.IsNullOrWhiteSpace(Path.GetExtension(preferredFileName)))
                {
                    var ext = MimeTypeHelper.TryGetExtensionFromMimeType(mimeFromServer) ?? ".bin";
                    preferredFileName += ext;
                }
            }
            else
            {
                var ext = MimeTypeHelper.TryGetExtensionFromMimeType(mimeFromServer) ?? ".bin";
                preferredFileName = uid + ext;
            }

            var targetPathResult = DownloadHelper.ResolveDownloadTargetPath(uid, saveTo, preferredFileName);
            if (!targetPathResult.IsSuccess || string.IsNullOrWhiteSpace(targetPathResult.Value))
            {
                return Result<string>.Fail(
                    targetPathResult.Error ?? Error.Validation("invalid_path", "The target download path is invalid."),
                    null,
                    null,
                    null);
            }

            var filePath = targetPathResult.Value!;

            var path = $"{this._segment}/download";
            var body = new { UID = uid };

            var wireResult = await this._ctx.Transport
                .PostJsonForStreamAsync(path, body, ct)
                .ConfigureAwait(false);

            if (!wireResult.IsSuccess || wireResult.Value is null)
            {
                return Result<string>.Fail(
                    wireResult.Error ?? Error.Unknown("Download failed."),
                    wireResult.StatusCode,
                    wireResult.CorrelationId,
                    wireResult.DurationMs);
            }

            var stream = wireResult.Value;

            try
            {
                var dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                using (stream)
                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await stream.CopyToAsync(fs, 81920, ct).ConfigureAwait(false);
                }

                return Result<string>.Success(
                    filePath,
                    wireResult.StatusCode,
                    wireResult.CorrelationId,
                    wireResult.DurationMs);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                stream.Dispose();

                return Result<string>.Fail(
                    Error.Unknown("Failed to write file to disk."),
                    wireResult.StatusCode,
                    wireResult.CorrelationId,
                    wireResult.DurationMs);
            }
        }

        public async Task<Result<IReadOnlyList<FileMetadata>>> ListFilesAsync(string? uid = null, int pageSize = 0, int pageNo = 0, bool fromSummary = true, bool totalReq = true, CancellationToken ct = default)
        {
            var path = "query/get";

            var query = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["fromSummary"] = fromSummary.ToString().ToLowerInvariant(),
                ["totalReq"] = totalReq.ToString().ToLowerInvariant(),
                ["pageSize"] = pageSize.ToString(CultureInfo.InvariantCulture),
                ["pageNo"] = pageNo.ToString(CultureInfo.InvariantCulture),
            };

            object payload = string.IsNullOrWhiteSpace(uid)
                ? new { }
                : new { UID = uid };

            var headers = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["ETId"] = "17",
            };

            var wireResult = await this._ctx.Transport
                .PostJsonWithHeadersAsync<object, List<FileMetadata>>(
                    path,
                    payload,
                    query,
                    headers,
                    ct)
                .ConfigureAwait(false);

            if (!wireResult.IsSuccess || wireResult.Value is null)
            {
                return Result<IReadOnlyList<FileMetadata>>.Fail(
                    wireResult.Error ?? Error.Unknown("List files failed."),
                    wireResult.StatusCode,
                    wireResult.CorrelationId,
                    wireResult.DurationMs);
            }

            var list = wireResult.Value;

            var ro = list.ToList().AsReadOnly();

            return Result<IReadOnlyList<FileMetadata>>.Success(
                ro,
                wireResult.StatusCode,
                wireResult.CorrelationId,
                wireResult.DurationMs);
        }
    }
}
