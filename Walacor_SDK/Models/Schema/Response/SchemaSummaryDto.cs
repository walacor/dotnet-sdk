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

using Newtonsoft.Json;

namespace Walacor_SDK.Models.Schema.Response
{
    public sealed class SchemaSummaryDto
    {
        public string UID { get; set; } = string.Empty;

        [JsonProperty("schema")]
        public string Schema { get; set; } = string.Empty;

        public int ETId { get; set; }

        [JsonProperty("createdDate")]
        public long CreatedDate { get; set; }

        public string Family { get; set; } = string.Empty;

        public int SV { get; set; }

        [JsonProperty("numberOfFields")]
        public int NumberOfFields { get; set; }
    }
}
