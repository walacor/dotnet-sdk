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

namespace Walacor_SDK.W_Client.Helpers
{
    public static class UriHelper
    {
        public static Uri NormalizeBaseUri(string baseUri)
        {
            if (!baseUri.Contains("://"))
            {
                baseUri = "http://" + baseUri;
            }

            var uri = new Uri(baseUri, UriKind.Absolute);
            var builder = new UriBuilder(uri)
            {
                Path = EnsureTrailingSlash(uri.AbsolutePath),
            };
            return builder.Uri;
        }

        public static Uri BuildApiBaseUri(Uri normalizedBase, string apiSegment)
        {
            var path = EnsureTrailingSlash(normalizedBase.AbsolutePath);
            var combined = CombinePaths(path, apiSegment);
            var builder = new UriBuilder(normalizedBase) { Path = combined };
            return builder.Uri;
        }

        public static string EnsureTrailingSlash(string path)
        {
            return string.IsNullOrEmpty(path)
                ? "/"
                : path.EndsWith("/", StringComparison.Ordinal) ? path : path + "/";
        }

        public static string CombinePaths(string left, string right)
        {
            if (string.IsNullOrEmpty(left))
            {
                left = "/";
            }

            if (left.EndsWith("/", StringComparison.Ordinal))
            {
                left = left.Substring(0, left.Length - 1);
            }

            if (string.IsNullOrEmpty(right))
            {
                right = string.Empty;
            }

            if (right.StartsWith("/", StringComparison.Ordinal))
            {
                right = right.Substring(1);
            }

            return left + "/" + right;
        }
    }
}
