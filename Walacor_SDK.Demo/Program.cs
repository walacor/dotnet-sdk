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

class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Hello Walacor!");
        var svc = new WalacorService("44.203.135.89", "Admin", "GreenDoor99");

        var dataTypes = await svc.SchemaService.GetDataTypesAsync();
        await Walacor_SDK.Tester.CallMeAsync();

    }
}
