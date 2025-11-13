using FluentAssertions;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Test_Walacor_SDK.W_Client.Helpers;
using Walacor_SDK.Client.Pipeline;
using Xunit;

namespace Test_Walacor_SDK.W_Client.Handlers
{
    public class CorrelationLoggingHandlerTests
    {
        [Fact]
        public async Task Sets_XRequestId_and_attaches_correlation_to_HttpRequestException()
        {
            var fake = new FakeHttpHandler();
            fake.Enqueue(new HttpRequestException("Simulated network failure"));

            var corr = new CorrelationLoggingHandler(fake);
            var client = new HttpClient(corr) { BaseAddress = new Uri("https://example.test/") };

            var ex = await Assert.ThrowsAsync<HttpRequestException>(async () =>
                await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "boom")));

            // Outbound request got an X-Request-ID
            fake.Requests.Should().NotBeEmpty();
            var requestId = fake.Requests[0].Headers.TryGetValues("X-Request-ID", out var vals)
                ? vals.FirstOrDefault()
                : null;
            requestId.Should().NotBeNullOrWhiteSpace();

            // Handler attached correlation + duration to exception data
            ex.Data["Walacor.CorrelationId"].Should().NotBeNull();
            ex.Data["Walacor.CorrelationId"]!.ToString().Should().Be(requestId);
            ex.Data["Walacor.Duration"].Should().NotBeNull();  // milliseconds elapsed
        }
    }
}
