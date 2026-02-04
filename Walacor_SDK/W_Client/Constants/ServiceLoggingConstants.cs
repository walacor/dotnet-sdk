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
    internal static class ServiceLoggingConstants
    {
        public const string MsgServiceCreated =
            "Service instantiated {ServiceName}";

        public const string MsgMethodEntry =
            "Service call {Service}.{Operation}";

        public const string MsgMethodEntryWithParam =
            "Service call {Service}.{Operation} ({ParamName}: {ParamValue})";

        public const string MsgMethodEntryWithParams =
            "Service call {Service}.{Operation} ({ParamName1}: {ParamValue1}, {ParamName2}: {ParamValue2})";

        public const string MsgMethodEntryWithParams3 =
            "Service call {Service}.{Operation} ({ParamName1}: {ParamValue1}, {ParamName2}: {ParamValue2}, {ParamName3}: {ParamValue3})";

        public const string MsgMethodEntryWithParams4 =
            "Service call {Service}.{Operation} ({ParamName1}: {ParamValue1}, {ParamName2}: {ParamValue2}, {ParamName3}: {ParamValue3}, {ParamName4}: {ParamValue4})";

        public const string MsgMethodEntryWithParams5 =
            "Service call {Service}.{Operation} ({ParamName1}: {ParamValue1}, {ParamName2}: {ParamValue2}, {ParamName3}: {ParamValue3}, {ParamName4}: {ParamValue4}, {ParamName5}: {ParamValue5})";

        public const string MsgMethodSuccess =
            "Service call completed {Service}.{Operation}";

        public const string MsgMethodFailure =
            "Service call failed {Service}.{Operation} (Code: {ErrorCode}, Message: {ErrorMessage})";

        public const string ParamEnvelopeTypeId = "EnvelopeTypeId";
        public const string ParamSchemaVersion = "SchemaVersion";
        public const string ParamDataVersion = "DataVersion";
        public const string ParamRecordId = "RecordId";
        public const string ParamUid = "Uid";
        public const string ParamTableName = "TableName";
        public const string ParamPageNumber = "PageNumber";
        public const string ParamPageSize = "PageSize";
        public const string ParamFromSummary = "FromSummary";
        public const string ParamTotalReq = "TotalReq";
        public const string ParamRecordsCount = "RecordsCount";
        public const string ParamSaveToProvided = "SaveToProvided";
        public const string ParamPipelineCount = "PipelineCount";
        public const string ParamFileName = "FileName";

        public const string MsgMethodFailureWithWire =
         "Service call failed {Service}.{Operation} " +
         "(Path: {Path}, Status: {StatusCode}, CorrelationId: {CorrelationId}, DurationMs: {DurationMs}, " +
         "Code: {ErrorCode}, Message: {ErrorMessage})";

        public const string ParamPath = "Path";
        public const string ParamStatusCode = "StatusCode";
        public const string ParamCorrelationId = "CorrelationId";
        public const string ParamDurationMs = "DurationMs";
    }
}
