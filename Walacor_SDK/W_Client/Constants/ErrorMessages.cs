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

namespace Walacor_SDK.W_Client.Constants
{
    internal static class ErrorMessages
    {
        // Validation / common
        public const string FileToVerifyDoesNotExist = "The file to verify does not exist.";
        public const string FileNotFound = "File not found.";
        public const string FileWasDeleted = "File was deleted.";
        public const string TargetDownloadPathInvalid = "The target download path is invalid.";

        public const string VerificationFailed = "Verification failed.";
        public const string DownloadFailed = "Download failed.";
        public const string ListFilesFailed = "List files failed.";
        public const string FailedToWriteFile = "Failed to write file to disk.";

        public const string UidRequiredToUpdate = "UID is required to update a record.";
        public const string RecordsAtLeastOneRequired = "At least one record is required.";
        public const string AllRecordsMustContainUid = "All records must contain a UID field.";

        public const string VerifyResponseMissingFileInfo = "Verify response did not contain 'fileInfo'.";
        public const string AuthSucceededNoToken = "Authentication succeeded but no token was returned.";
    }
}
