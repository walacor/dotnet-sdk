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
        private const string LocalPathMarker = "local";

        private readonly ClientContext _ctx;
        private readonly string _segment;
        private readonly ILogger _logger;

        public FileRequestsService(ClientContext ctx, string segment = ApiSegments.FilesV2)
        {
            this._ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
            this._segment = string.IsNullOrWhiteSpace(segment) ? ApiSegments.FilesV2 : segment.Trim('/');
            this._logger = this._ctx.Options.LoggerFactory.CreateLogger<FileRequestsService>();

            this._logger.LogDebug(
                ServiceLoggingConstants.MsgServiceCreated,
                nameof(FileRequestsService));
        }

#pragma warning disable MA0051 // Method is too long
        public async Task<Result<FileVerificationResult>> VerifyAsync(VerifySingleFileRequest request, CancellationToken ct = default)
#pragma warning restore MA0051 // Method is too long
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var sw = Stopwatch.StartNew();

            this._logger.LogDebug(
                ServiceLoggingConstants.MsgMethodEntryWithParam,
                nameof(FileRequestsService),
                nameof(this.VerifyAsync),
                ServiceLoggingConstants.ParamFileName,
                request.FileName);

            try
            {
                var fullPath = Path.GetFullPath(request.Path);

                if (!File.Exists(fullPath))
                {
                    sw.Stop();

                    this._logger.LogWarning(
                        ServiceLoggingConstants.MsgMethodFailureWithWire,
                        nameof(FileRequestsService),
                        nameof(this.VerifyAsync),
                        LocalPathMarker,
                        null,
                        string.Empty,
                        sw.ElapsedMilliseconds,
                        ErrorCodes.FileNotFound,
                        ErrorMessages.FileToVerifyDoesNotExist);

                    return Result<FileVerificationResult>.Fail(
                        Error.Validation(ErrorCodes.FileNotFound, ErrorMessages.FileToVerifyDoesNotExist),
                        null,
                        null,
                        sw.ElapsedMilliseconds);
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
                    sw.Stop();

                    this._logger.LogError(
                        ServiceLoggingConstants.MsgMethodFailureWithWire,
                        nameof(FileRequestsService),
                        nameof(this.VerifyAsync),
                        path,
                        wireResult.StatusCode,
                        wireResult.CorrelationId ?? string.Empty,
                        wireResult.DurationMs ?? sw.ElapsedMilliseconds,
                        wireResult.Error?.Code ?? "UNKNOWN",
                        wireResult.Error?.Message ?? ErrorMessages.VerificationFailed);

                    return Result<FileVerificationResult>.Fail(
                        wireResult.Error ?? Error.Unknown(ErrorMessages.VerificationFailed),
                        wireResult.StatusCode,
                        wireResult.CorrelationId,
                        wireResult.DurationMs ?? sw.ElapsedMilliseconds);
                }

                var dto = wireResult.Value;

                if (dto.FileInfo is null)
                {
                    sw.Stop();

                    this._logger.LogError(
                        ServiceLoggingConstants.MsgMethodFailureWithWire,
                        nameof(FileRequestsService),
                        nameof(this.VerifyAsync),
                        path,
                        wireResult.StatusCode,
                        wireResult.CorrelationId ?? string.Empty,
                        wireResult.DurationMs ?? sw.ElapsedMilliseconds,
                        "DESERIALIZATION",
                        ErrorMessages.VerifyResponseMissingFileInfo);

                    return Result<FileVerificationResult>.Fail(
                        Error.Deserialization(ErrorMessages.VerifyResponseMissingFileInfo),
                        wireResult.StatusCode,
                        wireResult.CorrelationId,
                        wireResult.DurationMs ?? sw.ElapsedMilliseconds);
                }

                var verification = FileVerificationResult.FromFileInfo(dto.FileInfo);

                this._logger.LogInformation(
                    ServiceLoggingConstants.MsgMethodSuccess,
                    nameof(FileRequestsService),
                    nameof(this.VerifyAsync));

                return Result<FileVerificationResult>.Success(
                    verification,
                    wireResult.StatusCode,
                    wireResult.CorrelationId,
                    wireResult.DurationMs ?? sw.ElapsedMilliseconds);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                sw.Stop();

                // Cancellation is expected behavior. Log as Debug to avoid noise.
                this._logger.LogDebug(
                    ServiceLoggingConstants.MsgMethodFailureWithWire,
                    nameof(FileRequestsService),
                    nameof(this.VerifyAsync),
                    LocalPathMarker,
                    null,
                    string.Empty,
                    sw.ElapsedMilliseconds,
                    "CANCELLED",
                    "Operation cancelled.");

                throw;
            }
        }

#pragma warning disable MA0051 // Method is too long
        public async Task<Result<StoreFileData>> StoreAsync(FileInfo fileInfo, CancellationToken ct = default)
#pragma warning restore MA0051 // Method is too long
        {
            if (fileInfo is null)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }

            var sw = Stopwatch.StartNew();

            // We don't assume FileInfo has a safe ID property here; log method entry without extra params.
            this._logger.LogDebug(
                ServiceLoggingConstants.MsgMethodEntry,
                nameof(FileRequestsService),
                nameof(this.StoreAsync));

            var path = $"{this._segment}/{ApiRoutes.Store}";
            var payload = new StoreFileRequest(fileInfo);

            try
            {
                var wireResult = await this._ctx.Transport
                    .PostJsonAsync<StoreFileRequest, StoreFileData>(path, payload, ct)
                    .ConfigureAwait(false);

                if (!wireResult.IsSuccess || wireResult.Value is null)
                {
                    sw.Stop();

                    this._logger.LogError(
                        ServiceLoggingConstants.MsgMethodFailureWithWire,
                        nameof(FileRequestsService),
                        nameof(this.StoreAsync),
                        path,
                        wireResult.StatusCode,
                        wireResult.CorrelationId ?? string.Empty,
                        wireResult.DurationMs ?? sw.ElapsedMilliseconds,
                        wireResult.Error?.Code ?? "UNKNOWN",
                        wireResult.Error?.Message ?? ErrorMessages.StoreFailed);

                    return Result<StoreFileData>.Fail(
                        wireResult.Error ?? Error.Unknown(ErrorMessages.StoreFailed),
                        wireResult.StatusCode,
                        wireResult.CorrelationId,
                        wireResult.DurationMs ?? sw.ElapsedMilliseconds);
                }

                this._logger.LogInformation(
                    ServiceLoggingConstants.MsgMethodSuccess,
                    nameof(FileRequestsService),
                    nameof(this.StoreAsync));

                return Result<StoreFileData>.Success(
                    wireResult.Value,
                    wireResult.StatusCode,
                    wireResult.CorrelationId,
                    wireResult.DurationMs ?? sw.ElapsedMilliseconds);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                sw.Stop();

                this._logger.LogDebug(
                    ServiceLoggingConstants.MsgMethodFailureWithWire,
                    nameof(FileRequestsService),
                    nameof(this.StoreAsync),
                    path,
                    null,
                    string.Empty,
                    sw.ElapsedMilliseconds,
                    "CANCELLED",
                    "Operation cancelled.");

                throw;
            }
        }

#pragma warning disable MA0051 // Method is too long
        public async Task<Result<string>> DownloadAsync(string uid, string? saveTo = null, CancellationToken ct = default)
#pragma warning restore MA0051 // Method is too long
        {
            if (string.IsNullOrWhiteSpace(uid))
            {
                throw new ArgumentNullException(nameof(uid));
            }

            var sw = Stopwatch.StartNew();

            this._logger.LogDebug(
                ServiceLoggingConstants.MsgMethodEntryWithParam,
                nameof(FileRequestsService),
                nameof(this.DownloadAsync),
                ServiceLoggingConstants.ParamUid,
                uid);

            try
            {
                var listResult = await this.ListFilesAsync(uid: uid, ct: ct).ConfigureAwait(false);

                if (!listResult.IsSuccess || listResult.Value is null || listResult.Value.Count == 0)
                {
                    sw.Stop();

                    this._logger.LogWarning(
                        ServiceLoggingConstants.MsgMethodFailureWithWire,
                        nameof(FileRequestsService),
                        nameof(this.DownloadAsync),
                        LocalPathMarker,
                        listResult.StatusCode,
                        listResult.CorrelationId ?? string.Empty,
                        listResult.DurationMs ?? sw.ElapsedMilliseconds,
                        listResult.Error?.Code ?? ErrorCodes.FileNotFound,
                        listResult.Error?.Message ?? ErrorMessages.FileNotFound);

                    return Result<string>.Fail(
                        listResult.Error ?? Error.NotFound(ErrorMessages.FileNotFound),
                        listResult.StatusCode,
                        listResult.CorrelationId,
                        listResult.DurationMs ?? sw.ElapsedMilliseconds);
                }

                var meta = listResult.Value.FirstOrDefault();
                if (meta is null)
                {
                    sw.Stop();

                    this._logger.LogWarning(
                        ServiceLoggingConstants.MsgMethodFailureWithWire,
                        nameof(FileRequestsService),
                        nameof(this.DownloadAsync),
                        LocalPathMarker,
                        listResult.StatusCode,
                        listResult.CorrelationId ?? string.Empty,
                        listResult.DurationMs ?? sw.ElapsedMilliseconds,
                        ErrorCodes.FileNotFound,
                        ErrorMessages.FileNotFound);

                    return Result<string>.Fail(
                        Error.NotFound(ErrorMessages.FileNotFound),
                        listResult.StatusCode,
                        listResult.CorrelationId,
                        listResult.DurationMs ?? sw.ElapsedMilliseconds);
                }

                if (!string.Equals(meta.Status, FileConstants.StoredStatus, StringComparison.OrdinalIgnoreCase))
                {
                    sw.Stop();

                    var msg = ErrorMessageFactory.FileNotReady(meta.Status);

                    this._logger.LogWarning(
                        ServiceLoggingConstants.MsgMethodFailureWithWire,
                        nameof(FileRequestsService),
                        nameof(this.DownloadAsync),
                        LocalPathMarker,
                        listResult.StatusCode,
                        listResult.CorrelationId ?? string.Empty,
                        listResult.DurationMs ?? sw.ElapsedMilliseconds,
                        ErrorCodes.FileNotReady,
                        msg);

                    return Result<string>.Fail(
                        Error.Validation(ErrorCodes.FileNotReady, msg),
                        listResult.StatusCode,
                        listResult.CorrelationId,
                        listResult.DurationMs ?? sw.ElapsedMilliseconds);
                }

                if (meta.IsDeleted == true)
                {
                    sw.Stop();

                    this._logger.LogWarning(
                        ServiceLoggingConstants.MsgMethodFailureWithWire,
                        nameof(FileRequestsService),
                        nameof(this.DownloadAsync),
                        LocalPathMarker,
                        listResult.StatusCode,
                        listResult.CorrelationId ?? string.Empty,
                        listResult.DurationMs ?? sw.ElapsedMilliseconds,
                        ErrorCodes.FileNotFound,
                        ErrorMessages.FileWasDeleted);

                    return Result<string>.Fail(
                        Error.NotFound(ErrorMessages.FileWasDeleted),
                        listResult.StatusCode,
                        listResult.CorrelationId,
                        listResult.DurationMs ?? sw.ElapsedMilliseconds);
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
                    sw.Stop();

                    this._logger.LogWarning(
                        ServiceLoggingConstants.MsgMethodFailureWithWire,
                        nameof(FileRequestsService),
                        nameof(this.DownloadAsync),
                        LocalPathMarker,
                        null,
                        string.Empty,
                        sw.ElapsedMilliseconds,
                        targetPathResult.Error?.Code ?? ErrorCodes.InvalidPath,
                        targetPathResult.Error?.Message ?? ErrorMessages.TargetDownloadPathInvalid);

                    return Result<string>.Fail(
                        targetPathResult.Error ?? Error.Validation(ErrorCodes.InvalidPath, ErrorMessages.TargetDownloadPathInvalid),
                        null,
                        null,
                        sw.ElapsedMilliseconds);
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
                    sw.Stop();

                    this._logger.LogError(
                        ServiceLoggingConstants.MsgMethodFailureWithWire,
                        nameof(FileRequestsService),
                        nameof(this.DownloadAsync),
                        path,
                        wireResult.StatusCode,
                        wireResult.CorrelationId ?? string.Empty,
                        wireResult.DurationMs ?? sw.ElapsedMilliseconds,
                        wireResult.Error?.Code ?? "UNKNOWN",
                        wireResult.Error?.Message ?? ErrorMessages.DownloadFailed);

                    return Result<string>.Fail(
                        wireResult.Error ?? Error.Unknown(ErrorMessages.DownloadFailed),
                        wireResult.StatusCode,
                        wireResult.CorrelationId,
                        wireResult.DurationMs ?? sw.ElapsedMilliseconds);
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

                    this._logger.LogInformation(
                        ServiceLoggingConstants.MsgMethodSuccess,
                        nameof(FileRequestsService),
                        nameof(this.DownloadAsync));

                    return Result<string>.Success(
                        filePath,
                        wireResult.StatusCode,
                        wireResult.CorrelationId,
                        wireResult.DurationMs ?? sw.ElapsedMilliseconds);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    sw.Stop();

                    this._logger.LogError(
                        ServiceLoggingConstants.MsgMethodFailureWithWire,
                        nameof(FileRequestsService),
                        nameof(this.DownloadAsync),
                        LocalPathMarker,
                        wireResult.StatusCode,
                        wireResult.CorrelationId ?? string.Empty,
                        wireResult.DurationMs ?? sw.ElapsedMilliseconds,
                        "IO_WRITE_FAILED",
                        ErrorMessages.FailedToWriteFile);

                    return Result<string>.Fail(
                        Error.Unknown(ErrorMessages.FailedToWriteFile),
                        wireResult.StatusCode,
                        wireResult.CorrelationId,
                        wireResult.DurationMs ?? sw.ElapsedMilliseconds);
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                sw.Stop();

                this._logger.LogDebug(
                    ServiceLoggingConstants.MsgMethodFailureWithWire,
                    nameof(FileRequestsService),
                    nameof(this.DownloadAsync),
                    LocalPathMarker,
                    null,
                    string.Empty,
                    sw.ElapsedMilliseconds,
                    "CANCELLED",
                    "Operation cancelled.");

                throw;
            }
        }

#pragma warning disable MA0051 // Method is too long
        public async Task<Result<IReadOnlyList<FileMetadata>>> ListFilesAsync(
            string? uid = null,
            int pageSize = 0,
            int pageNo = 0,
            bool fromSummary = true,
            bool totalReq = true,
            CancellationToken ct = default)
#pragma warning restore MA0051 // Method is too long
        {
            var sw = Stopwatch.StartNew();

            // Entry log (uid is optional)
            if (string.IsNullOrWhiteSpace(uid))
            {
                this._logger.LogDebug(
                    ServiceLoggingConstants.MsgMethodEntry,
                    nameof(FileRequestsService),
                    nameof(this.ListFilesAsync));
            }
            else
            {
                this._logger.LogDebug(
                    ServiceLoggingConstants.MsgMethodEntryWithParam,
                    nameof(FileRequestsService),
                    nameof(this.ListFilesAsync),
                    ServiceLoggingConstants.ParamUid,
                    uid);
            }

            var path = ApiRoutes.QueryGet;

            try
            {
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
                    sw.Stop();

                    this._logger.LogError(
                        ServiceLoggingConstants.MsgMethodFailureWithWire,
                        nameof(FileRequestsService),
                        nameof(this.ListFilesAsync),
                        path,
                        wireResult.StatusCode,
                        wireResult.CorrelationId ?? string.Empty,
                        wireResult.DurationMs ?? sw.ElapsedMilliseconds,
                        wireResult.Error?.Code ?? "UNKNOWN",
                        wireResult.Error?.Message ?? ErrorMessages.ListFilesFailed);

                    return Result<IReadOnlyList<FileMetadata>>.Fail(
                        wireResult.Error ?? Error.Unknown(ErrorMessages.ListFilesFailed),
                        wireResult.StatusCode,
                        wireResult.CorrelationId,
                        wireResult.DurationMs ?? sw.ElapsedMilliseconds);
                }

                var ro = wireResult.Value.ToList().AsReadOnly();

                this._logger.LogInformation(
                    ServiceLoggingConstants.MsgMethodSuccess,
                    nameof(FileRequestsService),
                    nameof(this.ListFilesAsync));

                return Result<IReadOnlyList<FileMetadata>>.Success(
                    ro,
                    wireResult.StatusCode,
                    wireResult.CorrelationId,
                    wireResult.DurationMs ?? sw.ElapsedMilliseconds);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                sw.Stop();

                this._logger.LogDebug(
                    ServiceLoggingConstants.MsgMethodFailureWithWire,
                    nameof(FileRequestsService),
                    nameof(this.ListFilesAsync),
                    path,
                    null,
                    string.Empty,
                    sw.ElapsedMilliseconds,
                    "CANCELLED",
                    "Operation cancelled.");

                throw;
            }
        }
    }
}
