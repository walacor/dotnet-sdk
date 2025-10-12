using FluentAssertions;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Test_Walacor_SDK.W_Client.Helpers;
using Walacor_SDK.Client;
using Walacor_SDK.Client.Exceptions;
using Walacor_SDK.Client.Serialization;
using Xunit;

namespace Test_Walacor_SDK.W_Client
{
    public class WalacorHttpClientTests
    {
        public class SampleReq
        {
            public string Name { get; set; } = "";
        }

        public class SampleResp
        {
            public bool Ok { get; set; }
        }


        [Fact]
        public async Task Throws_WalacorValidationException_on_422_when_enabled()
        {
            var fake = new FakeHttpHandler();
            var resp422 = new HttpResponseMessage((HttpStatusCode)422)
            { Content = new StringContent(@"{""errors"":{""Name"":[""Required""]}}") };
            fake.Enqueue(resp422);

            var http = new HttpClient(fake) { BaseAddress = new Uri("https://example.test/") };
            var json = new NewtonsoftJsonSerializer();
            var opts = new WalacorHttpClientOptions { ThrowOnValidation422 = true };

            var client = new WalacorHttpClient(http, json, opts);

            Func<Task> act = async () =>
                await client.PostJsonAsync<SampleReq, SampleResp>("items", new SampleReq { Name = "" });
            await act.Should().ThrowAsync<WalacorValidationException>();
        }

        [Fact]
        public async Task Does_not_throw_validation_exception_on_422_when_disabled()
        {
            var fake = new FakeHttpHandler();
            var resp = new HttpResponseMessage((HttpStatusCode)422) { Content = new StringContent(@"{""errors"":{}}") };
            fake.Enqueue(resp);

            var http = new HttpClient(fake) { BaseAddress = new Uri("https://example.test/") };
            var json = new NewtonsoftJsonSerializer();
            var opts = new WalacorHttpClientOptions { ThrowOnValidation422 = false };

            var client = new WalacorHttpClient(http, json, opts);

            Func<Task> act = async () =>
                await client.PostJsonAsync<SampleReq, SampleResp>("items", new SampleReq { Name = "x" }, default);
            await act.Should().NotThrowAsync<WalacorValidationException>();

        }
    }
}
