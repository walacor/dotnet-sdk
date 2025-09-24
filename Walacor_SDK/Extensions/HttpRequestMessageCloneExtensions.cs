// Copyright 2024 Walacor Corporation
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

using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Walacor_SDK.Extensions
{
    internal static class HttpRequestMessageCloneExtensions
    {
        public static async Task<HttpRequestMessage> CloneAsync(this HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri) { Version = request.Version };

            foreach (var h in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(h.Key, h.Value);
            }

            if (request.Content != null)
            {
                var ms = new MemoryStream();
                await request.Content.CopyToAsync(ms).ConfigureAwait(false);
                ms.Position = 0;

                var newContent = new StreamContent(ms);
                foreach (var h in request.Content.Headers)
                {
                    newContent.Headers.TryAddWithoutValidation(h.Key, h.Value);
                }

                clone.Content = newContent;
            }

            return clone;
        }
    }
}
