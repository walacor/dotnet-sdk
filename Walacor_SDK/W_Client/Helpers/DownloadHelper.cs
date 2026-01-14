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
using Walacor_SDK.Models.Result;
using Walacor_SDK.Models.Results;
using Walacor_SDK.W_Client.Constants;

namespace Walacor_SDK.W_Client.Helpers
{
    public class DownloadHelper
    {
        public static Result<string> ResolveDownloadTargetPath(string uid, string? saveTo)
        {
            try
            {
                string path;

                if (!string.IsNullOrWhiteSpace(saveTo) && HasFileExtension(saveTo))
                {
                    path = Path.GetFullPath(saveTo);
                }
                else
                {
                    var directory = !string.IsNullOrWhiteSpace(saveTo)
                        ? Path.GetFullPath(saveTo)
                        : GetDefaultDownloadDirectory();

                    path = Path.Combine(directory, uid);
                }

                return Result<string>.Success(path, null, null, null);
            }
            catch
            {
                return Result<string>.Fail(
                    Error.Validation(ErrorCodes.InvalidPath, ErrorMessages.TargetDownloadPathInvalid),
                    null,
                    null,
                    null);
            }
        }

        public static Result<string> ResolveDownloadTargetPath(string uid, string? saveTo, string? preferredFileName)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(saveTo) && HasFileExtension(saveTo))
                {
                    return Result<string>.Success(Path.GetFullPath(saveTo), null, null, null);
                }

                var directory = !string.IsNullOrWhiteSpace(saveTo)
                    ? Path.GetFullPath(saveTo)
                    : GetDefaultDownloadDirectory();

                string rawName = preferredFileName ?? uid;
                if (string.IsNullOrWhiteSpace(rawName))
                {
                    rawName = uid;
                }

                var safeName = SanitizeFileName(rawName);

                return Result<string>.Success(Path.Combine(directory, safeName), null, null, null);
            }
            catch
            {
                return Result<string>.Fail(
                    Error.Validation(ErrorCodes.InvalidPath, ErrorMessages.TargetDownloadPathInvalid),
                    null,
                    null,
                    null);
            }
        }

        private static bool HasFileExtension(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            var ext = Path.GetExtension(path);
            return !string.IsNullOrEmpty(ext);
        }

        private static string GetDefaultDownloadDirectory()
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (string.IsNullOrEmpty(home))
            {
                home = Directory.GetCurrentDirectory();
            }

            var downloads = Path.Combine(home, FileSystemNames.DownloadsDirectoryName);
            var walacor = Path.Combine(downloads, FileSystemNames.WalacorDirectoryName);
            return walacor;
        }

        private static string SanitizeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, FileSystemNames.InvalidFileNameReplacementChar);
            }

            return name;
        }
    }
}
