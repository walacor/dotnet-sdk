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
    public sealed class SchemaItemDto
    {
        [JsonProperty("_id")]
        public string Id { get; set; } = string.Empty;

        public string ORGId { get; set; } = string.Empty;

        public string ORGName { get; set; } = string.Empty;

        public string EId { get; set; } = string.Empty;

        public int ETId { get; set; }

        public string TableName { get; set; } = string.Empty;

        public string DbTableName { get; set; } = string.Empty;

        public string DbHistoryTableName { get; set; } = string.Empty;

        public string Family { get; set; } = string.Empty;

        public bool DoSummary { get; set; }

        public string LastModifiedBy { get; set; } = string.Empty;

        public long CreatedAt { get; set; }

        public long UpdatedAt { get; set; }

        public int? DV { get; set; }

        public string Description { get; set; } = string.Empty;
    }
}
