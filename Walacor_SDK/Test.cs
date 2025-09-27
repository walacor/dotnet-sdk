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
using System.Net.Http;
using System.Threading.Tasks;
using Walacor_SDK;
using Walacor_SDK.Abstractions;
using Walacor_SDK.Auth;
using Walacor_SDK.Serialization; // WalacorHttpClient (ensure this is your correct namespace)

#pragma warning disable MA0047
public static class Test
#pragma warning restore MA0047
{
    public static async Task CallMeAsync()
    {
        var uri = new Uri("https://api.walacor.com/");
        var serializer = new NewtonsoftJsonSerializer();

        // supply real credentials:
        IAuthTokenProvider tokenProvider = new UsernamePasswordTokenProvider(uri, "user", "pass");

        // use the ctor that matches your implementation:
        var client = new WalacorHttpClient(uri, tokenProvider); // if your class exposes this signature

        using var req = new HttpRequestMessage(HttpMethod.Get, "schema"); // e.g., GET https://api.walacor.com/schema
        using var res = await client.SendAsync(req).ConfigureAwait(false);
        var body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

        Console.WriteLine($"Status: {(int)res.StatusCode}");
        Console.WriteLine($"Body: {body}");
    }
}
