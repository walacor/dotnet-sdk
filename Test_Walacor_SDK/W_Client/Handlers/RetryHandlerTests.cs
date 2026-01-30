using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Test_Walacor_SDK.W_Client.Helpers;
using Walacor_SDK.Client.Pipeline;
using Xunit;

namespace Test_Walacor_SDK.W_Client.Handlers
{
    public class RetryHandlerTests
    {
        private static HttpClient BuildClient(HttpMessageHandler root)
            => new HttpClient(root) { BaseAddress = new Uri("https://example.test/") };

        [Fact]
        public async Task Retry_GET_with_RetryAfterDate_in_past_retries_once_no_sleep_and_preserves_body_and_headers()
        {
            var fake = new FakeHttpHandler();

            // 429 WITH Retry-After DATE in the past => zero wait, backoff NOT consulted
            var r1 = new HttpResponseMessage((HttpStatusCode)429);
            r1.Headers.Add("Retry-After", "Mon, 01 Jan 1990 00:00:00 GMT");
            fake.Enqueue(r1);

            // then success
            fake.Enqueue(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(@"{""ok"":true}") });

            var backoff = new DeterministicBackoff();
            var retry = new RetryHandler(backoff, maxRetries: 3, logger: NullLogger.Instance, inner: fake);
            var client = BuildClient(retry);

            var req = new HttpRequestMessage(HttpMethod.Get, "foo");
            req.Headers.Add("X-Custom", "abc");
            req.Content = new StringContent("payload");

            var resp = await client.SendAsync(req, CancellationToken.None);
            resp.StatusCode.Should().Be(HttpStatusCode.OK);

            fake.Requests.Should().HaveCount(2);
            fake.Bodies[0].Should().Be("payload");
            fake.Bodies[1].Should().Be("payload");
            fake.Requests[0].Headers.GetValues("X-Custom").Single().Should().Be("abc");
            fake.Requests[1].Headers.GetValues("X-Custom").Single().Should().Be("abc");

            // With Retry-After header present, handler should NOT call backoff.
            backoff.ComputeCalls.Should().Be(0);
        }

        [Fact]
        public async Task Retry_GET_without_RetryAfter_uses_backoff_once_then_succeeds()
        {
            var fake = new FakeHttpHandler();
            // 429 WITHOUT Retry-After => backoff consulted once
            fake.Enqueue(new HttpResponseMessage((HttpStatusCode)429));
            fake.Enqueue(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(@"{""ok"":true}") });

            var backoff = new DeterministicBackoff();
            var retry = new RetryHandler(backoff, maxRetries: 3, logger: NullLogger.Instance, inner: fake);
            var client = BuildClient(retry);

            var req = new HttpRequestMessage(HttpMethod.Get, "foo");
            req.Content = new StringContent("payload");

            var resp = await client.SendAsync(req, CancellationToken.None);
            resp.StatusCode.Should().Be(HttpStatusCode.OK);

            fake.Requests.Should().HaveCount(2);
            backoff.ComputeCalls.Should().Be(1);
        }

        [Fact]
        public async Task No_retry_for_POST_even_on_429()
        {
            var fake = new FakeHttpHandler();
            fake.Enqueue(new HttpResponseMessage((HttpStatusCode)429));

            var retry = new RetryHandler(new DeterministicBackoff(), maxRetries: 3, logger: NullLogger.Instance, inner: fake);
            var client = BuildClient(retry);

            var req = new HttpRequestMessage(HttpMethod.Post, "bar") { Content = new StringContent("x") };
            var resp = await client.SendAsync(req);
            resp.StatusCode.Should().Be((HttpStatusCode)429);
            fake.Requests.Should().HaveCount(1);
        }
    }
}
