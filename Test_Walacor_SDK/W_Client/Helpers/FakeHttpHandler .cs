// Test_Walacor_SDK/W_Client/Helpers/FakeHttpHandler.cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Test_Walacor_SDK.W_Client.Helpers
{
    public sealed class FakeHttpHandler : HttpMessageHandler
    {
        private readonly ConcurrentQueue<object> _script = new();
        public List<HttpRequestMessage> Requests { get; } = new();
        public List<string?> Bodies { get; } = new();  

        public void Enqueue(HttpResponseMessage response) => _script.Enqueue(response);
        public void Enqueue(Exception ex) => _script.Enqueue(ex);
        public void Enqueue(Func<HttpRequestMessage, HttpResponseMessage> responder) => _script.Enqueue(responder);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            // snapshot body BEFORE anyone can dispose/mutate 
            string? body = request.Content is null ? null : await request.Content.ReadAsStringAsync();
            Requests.Add(ShallowCopy(request));
            Bodies.Add(body);

            if (!_script.TryDequeue(out var next))
                throw new InvalidOperationException("FakeHttpHandler script exhausted.");

            return next switch
            {
                HttpResponseMessage resp => resp,
                Exception ex => throw ex,
                Func<HttpRequestMessage, HttpResponseMessage> f => f(request),
                _ => throw new InvalidOperationException("Unsupported script item.")
            };
        }

        private static HttpRequestMessage ShallowCopy(HttpRequestMessage req)
        {
            var copy = new HttpRequestMessage(req.Method, req.RequestUri);
            foreach (var h in req.Headers) copy.Headers.TryAddWithoutValidation(h.Key, h.Value);
            copy.Content = req.Content; 
            return copy;
        }
    }
}
