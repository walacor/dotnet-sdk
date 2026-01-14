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
using System.IO;

namespace Walacor_SDK.W_Client.Helpers
{
    internal static class MimeTypeHelper
    {
        private static readonly IDictionary<string, string> Map =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { ".txt",  "text/plain" },
                { ".json", "application/json" },
                { ".csv",  "text/csv" },
                { ".xml",  "application/xml" },
                { ".jpg",  "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".png",  "image/png" },
                { ".gif",  "image/gif" },
                { ".pdf",  "application/pdf" },
            };

        public static string? TryGetExtensionFromMimeType(string? mimeType)
        {
            if (string.IsNullOrWhiteSpace(mimeType))
            {
                return null;
            }

            var mt = mimeType?.Split(';')[0].Trim();

            foreach (var kv in Map)
            {
                if (string.Equals(kv.Value, mt, StringComparison.OrdinalIgnoreCase))
                {
                    return kv.Key; // ".png"
                }
            }

            return null;
        }

        public static string GetMimeType(string fileName, string fallback)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return fallback;
            }

            var ext = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(ext))
            {
                return fallback;
            }

            string value;
            if (Map.TryGetValue(ext, out value))
            {
                return value;
            }

            return fallback;
        }
    }
}
