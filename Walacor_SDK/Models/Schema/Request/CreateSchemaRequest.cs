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

using Walacor_SDK.Enums;

namespace Walacor_SDK.Models.Schema.Request
{
    public class CreateSchemaRequest
    {
        public SystemEnvelopeType ETId { get; set; } = SystemEnvelopeType.Schema;

        public int SV { get; set; } = 1;

        public CreateSchemaDefinition Schema { get; set; } = default!;
    }
}
