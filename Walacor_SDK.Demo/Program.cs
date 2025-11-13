// Copyright 2025 Walacor Corporation
//
// Licensed under the Apache License, Version 2.0 (the "License")
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

using Walacor_SDK;
using Walacor_SDK.Enums;
using Walacor_SDK.Models.Schema.Request;

class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Hello Walacor!");
        var svc = new WalacorService("x", "y", "z");

        //var dataTypes = await svc.SchemaService.GetDataTypesAsync();
        //var pagf = await svc.SchemaService.GetPlatformAutoGenerationFieldsAsync();
        //var llv = await svc.SchemaService.GetListWithLatestVersionAsync();
        //var versions = await svc.SchemaService.GetVersionsAsync();
        //var versions_etid = await svc.SchemaService.GetVersionsForEnvelopeTypeAsync(10000);
        //var indexes = await svc.SchemaService.GetIndexesAsync(15);
        //var indexesEnum = await svc.SchemaService.GetIndexesAsync(SystemEnvelopeType.OrgId);
        //var indexesByName = await svc.SchemaService.GetIndexesByTableNameAsync("roles");
        //var schema = await svc.SchemaService.CreateSchemaAsync(BuildSchemaMode());
        //var details = await svc.SchemaService.GetDetailsByIdAsync("4459");
        //var schmeDetails = await svc.SchemaService.GetSchemaDetailsByEnvelopeTypeAsync(10);
        //var envTypes = await svc.SchemaService.GetEnvelopeTypesAsync();
        //var lsi = await svc.SchemaService.GetListSchemaItemsAsync();
        var sqsi = await svc.SchemaService.GetSchemaQuerySchemaItemsAsync(new SchemaQueryListRequest());


        //Console.WriteLine(dataTypes.Value);
        //Console.WriteLine(pagf.Value);
        //Console.WriteLine(llv.Value);
        //Console.WriteLine(versions.Value);
        //Console.WriteLine(versions_etid.Value);
        //Console.WriteLine(indexes.Value);
        //Console.WriteLine(indexesEnum.Value);
        //Console.WriteLine(indexesByName.Value);
        //Console.WriteLine(schema.Value);
        //Console.WriteLine(details.Value);
        //Console.WriteLine(schmeDetails.Value);
        //Console.WriteLine(envTypes.Value);
    }

    public static CreateSchemaRequest BuildSchemaMode()
    {
        return new CreateSchemaRequest()
        {
            Schema = new CreateSchemaDefinition()
            {
                ETId = 11223344,
                TableName = "dotnet",
                Family = "sdk",
                DoSummary = true,
                Fields = new List<CreateFieldRequest>()
                {
                    new CreateFieldRequest(){
                        DataType = FieldType.Text,
                        FieldName = "version",
                        MaxLength = 5,
                        Required= true
                    },
                    new CreateFieldRequest(){
                        DataType= FieldType.Boolean,
                        FieldName= "deployed",
                        Default = true,
                        Required = true
                    },
                },
                Indexes = new List<CreateIndexRequest>()
                {
                    new CreateIndexRequest(){
                        Fields = ["version"],
                        IndexValue = "versions",
                        ForceUpdate = false,
                        Delete = false,
                    },

                },
            }
        };
    }
}
