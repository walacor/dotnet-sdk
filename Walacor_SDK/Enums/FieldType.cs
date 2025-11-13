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

using System.Runtime.Serialization;

namespace Walacor_SDK.Enums
{
    /// <summary>
    /// Defines supported field data types.
    /// </summary>
    public enum FieldType
    {
        /// <summary>
        /// Integer numeric value.
        /// </summary>
        [EnumMember(Value = "INTEGER")]
        Integer,

        /// <summary>
        /// Text (string) value.
        /// </summary>
        [EnumMember(Value = "TEXT")]
        Text,

        /// <summary>
        /// Decimal (floating precision) numeric value.
        /// </summary>
        [EnumMember(Value = "DECIMAL")]
        Decimal,

        /// <summary>
        /// Boolean (true/false) value.
        /// </summary>
        [EnumMember(Value = "BOOLEAN")]
        Boolean,

        /// <summary>
        /// Date/time stored as a Unix epoch timestamp.
        /// </summary>
        [EnumMember(Value = "DATETIME(EPOCH)")]
        DateTimeEpoch,

        /// <summary>
        /// Array of values.
        /// </summary>
        [EnumMember(Value = "ARRAY")]
        Array,

        /// <summary>
        /// Cron expression (schedule syntax).
        /// </summary>
        [EnumMember(Value = "CRON")]
        Cron,
    }
}
