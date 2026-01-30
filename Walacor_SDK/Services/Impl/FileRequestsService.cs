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
using Microsoft.Extensions.Logging;
using Walacor_SDK.Models.FileRequests.Request;
using Walacor_SDK.Models.FileRequests.Response;
using Walacor_SDK.Models.Result;
using Walacor_SDK.Models.Results;
using Walacor_SDK.Services.Abs;
using Walacor_SDK.W_Client.Constants;
using Walacor_SDK.W_Client.Context;
using Walacor_SDK.W_Client.Helpers;
using FileInfo = Walacor_SDK.Models.FileRequests.Response.FileInfo;

namespace Walacor_SDK.Services.Impl
{
    internal class FileRequestsService : IFileRequestsService
    {
        private readonly ClientContext _ctx;
        private readonly string _segment;
        private readonly ILogger _logger;

        public FileRequestsService(ClientContext ctx, string segment = ApiSegments.FilesV2)
        {
            this._ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
            this._segment = string.IsNullOrWhiteSpace(segment) ? ApiSegments.FilesV2 : segment.Trim('/');
            this._logger = this._ctx.Options.LoggerFactory.CreateLogger("Walacor_SDK.Service.FileReuestsService");
        }

        public async Task<Result<FileVerificationResult>> VerifyAsync(VerifySingleFileRequest request, CancellationToken ct = default)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var fullPath = Path.GetFullPath(request.Path);

            if (!File.Exists(fullPath))
            {
                return Result<FileVerificationResult>.Fail(
                    Error.Validation(ErrorCodes.FileNotFound, ErrorMessages.FileToVerifyDoesNotExist),
                    null,
                    null);
            }

            var fileName = request.FileName;
            var mimeType = request.MimeType
                            ?? MimeTypeHelper.GetMimeType(fileName, MediaTypeNames.ApplicationOctetStream);

            using var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);

            using var multipart = new MultipartFormDataContent();
            multipart.Add(fileContent, JsonFieldNames.MultipartFileField, fileName);

            var path = string.Concat(this._segment, "/", ApiRoutes.Verify);

            var wireResult = await this._ctx.Transport
                .PostMultipartAsync<VerifyResponseDto>(path, multipart, ct)
                .ConfigureAwait(false);

            if (!wireResult.IsSuccess || wireResult.Value is null)
            {
                return Result<FileVerificationResult>.Fail(
                    wireResult.Error ?? Error.Unknown(ErrorMessages.VerificationFailed),
                    wireResult.StatusCode,
                    wireResult.CorrelationId);
            }

            var dto = wireResult.Value;

            if (dto.FileInfo is null)
            {
                return Result<FileVerificationResult>.Fail(
                    Error.Deserialization(ErrorMessages.VerifyResponseMissingFileInfo),
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

            var path = $"{this._segment}/{ApiRoutes.Store}";
            var payload = new StoreFileRequest(fileInfo);

            return await this._ctx.Transport
                .PostJsonAsync<StoreFileRequest, StoreFileData>(path, payload, ct)
                .ConfigureAwait(false);
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
                    listResult.Error ?? Error.NotFound(ErrorMessages.FileNotFound),
                    listResult.StatusCode,
                    listResult.CorrelationId,
                    listResult.DurationMs);
            }

            FileMetadata? meta = listResult.Value.FirstOrDefault();
            if (meta is null)
            {
                return Result<string>.Fail(
                    Error.NotFound(ErrorMessages.FileNotFound),
                    listResult.StatusCode,
                    listResult.CorrelationId,
                    listResult.DurationMs);
            }

            if (string.Equals(meta.Status, FileConstants.StoredStatus, StringComparison.OrdinalIgnoreCase) == false)
            {
                return Result<string>.Fail(
                    Error.Validation(ErrorCodes.FileNotReady, ErrorMessageFactory.FileNotReady(meta.Status)),
                    listResult.StatusCode,
                    listResult.CorrelationId,
                    listResult.DurationMs);
            }

            if (meta.IsDeleted == true)
            {
                return Result<string>.Fail(
                    Error.NotFound(ErrorMessages.FileWasDeleted),
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
                    var ext = MimeTypeHelper.TryGetExtensionFromMimeType(mimeFromServer) ?? FileConstants.DefaultBinaryExtension;
                    preferredFileName += ext;
                }
            }
            else
            {
                var ext = MimeTypeHelper.TryGetExtensionFromMimeType(mimeFromServer) ?? FileConstants.DefaultBinaryExtension;
                preferredFileName = uid + ext;
            }

            var targetPathResult = DownloadHelper.ResolveDownloadTargetPath(uid, saveTo, preferredFileName);
            if (!targetPathResult.IsSuccess || string.IsNullOrWhiteSpace(targetPathResult.Value))
            {
                return Result<string>.Fail(
                    targetPathResult.Error ?? Error.Validation(ErrorCodes.InvalidPath, ErrorMessages.TargetDownloadPathInvalid),
                    null,
                    null,
                    null);
            }

            var filePath = targetPathResult.Value!;

            var path = $"{this._segment}/{ApiRoutes.Download}";
            var body = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [JsonFieldNames.UID] = uid,
            };

            var wireResult = await this._ctx.Transport
                .PostJsonForStreamAsync(path, body, ct)
                .ConfigureAwait(false);

            if (!wireResult.IsSuccess || wireResult.Value is null)
            {
                return Result<string>.Fail(
                    wireResult.Error ?? Error.Unknown(ErrorMessages.DownloadFailed),
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
                    await stream.CopyToAsync(fs, FileConstants.DefaultCopyBufferSize, ct).ConfigureAwait(false);
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
                    Error.Unknown(ErrorMessages.FailedToWriteFile),
                    wireResult.StatusCode,
                    wireResult.CorrelationId,
                    wireResult.DurationMs);
            }
        }

        public async Task<Result<IReadOnlyList<FileMetadata>>> ListFilesAsync(
            string? uid = null,
            int pageSize = 0,
            int pageNo = 0,
            bool fromSummary = true,
            bool totalReq = true,
            CancellationToken ct = default)
        {
            var path = ApiRoutes.QueryGet;

            this.LogRequest(nameof(this.ListFilesAsync), path);

            var query = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [QueryParamNames.FromSummary] = fromSummary.ToString().ToLowerInvariant(),
                [QueryParamNames.TotalReq] = totalReq.ToString().ToLowerInvariant(),
                [QueryParamNames.PageSize] = pageSize.ToString(CultureInfo.InvariantCulture),
                [QueryParamNames.PageNo] = pageNo.ToString(CultureInfo.InvariantCulture),
            };

            object payload = string.IsNullOrWhiteSpace(uid)
                ? new { }
                : new Dictionary<string, string>(StringComparer.Ordinal) { [JsonFieldNames.UID] = uid ?? string.Empty };

            var headers = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [HeaderNames.ETId] = "17",
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
                    wireResult.Error ?? Error.Unknown(ErrorMessages.ListFilesFailed),
                    wireResult.StatusCode,
                    wireResult.CorrelationId,
                    wireResult.DurationMs);
            }

            var ro = wireResult.Value.ToList().AsReadOnly();

            return Result<IReadOnlyList<FileMetadata>>.Success(
                ro,
                wireResult.StatusCode,
                wireResult.CorrelationId,
                wireResult.DurationMs);
        }

        private void LogRequest(string operation, string path)
        {
            this._logger.LogInformation(
                "SchemaService {Operation} {Path}",
                operation,
                path);
        }
    }
}
