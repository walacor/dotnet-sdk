using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Test_Walacor_SDK.W_Client.Helpers;
using Walacor_SDK.Client.Pipeline;
using Walacor_SDK.W_Client.Abstractions;
using Xunit;

namespace Test_Walacor_SDK.W_Client.Handlers
{
    public class AuthHandlerTests
    {
        [Fact]
        public async Task Initial_401_triggers_single_refresh_and_retries_with_new_token()
        {
            var fake = new FakeHttpHandler();
            fake.Enqueue(new HttpResponseMessage(HttpStatusCode.Unauthorized));
            fake.Enqueue(req =>
            {
                // Assert retried request carries refreshed token
                req.Headers.Authorization!.Scheme.Should().Be("Bearer");
                req.Headers.Authorization!.Parameter.Should().Be("new-token");
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

            var tokens = new Mock<IAuthTokenProvider>(MockBehavior.Strict);

            tokens.Setup(p => p.GetTokenAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync("old-token");
            tokens.Setup(p => p.RefreshTokenAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync("new-token");

            var auth = new AuthHandler(tokens.Object, NullLogger.Instance, inner: fake);
            var client = new HttpClient(auth) { BaseAddress = new Uri("https://example.test/") };

            var resp = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "secure"));
            resp.IsSuccessStatusCode.Should().BeTrue();

            tokens.Verify(p => p.GetTokenAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            tokens.Verify(p => p.RefreshTokenAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
