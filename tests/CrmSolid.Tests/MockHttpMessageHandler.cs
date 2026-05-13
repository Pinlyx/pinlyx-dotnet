using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrmSolid.Tests;

/// <summary>Test double for <see cref="HttpMessageHandler"/>. Records requests, returns scripted responses.</summary>
internal sealed class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _handler;

    public List<HttpRequestMessage> Requests { get; } = new();
    public List<string> RequestBodies { get; } = new();

    public MockHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
    {
        _handler = handler;
    }

    public MockHttpMessageHandler(HttpStatusCode status, string? body = null, IDictionary<string, string>? headers = null)
        : this(_ => Task.FromResult(BuildResponse(status, body, headers))) { }

    public static MockHttpMessageHandler Sequence(params HttpResponseMessage[] responses)
    {
        var queue = new Queue<HttpResponseMessage>(responses);
        return new MockHttpMessageHandler(_ =>
        {
            if (queue.Count == 0)
                throw new InvalidOperationException("No more scripted responses.");
            return Task.FromResult(queue.Dequeue());
        });
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Requests.Add(request);
        if (request.Content != null)
            RequestBodies.Add(await request.Content.ReadAsStringAsync().ConfigureAwait(false));
        return await _handler(request).ConfigureAwait(false);
    }

    public static HttpResponseMessage BuildResponse(
        HttpStatusCode status,
        string? body = null,
        IDictionary<string, string>? headers = null)
    {
        var response = new HttpResponseMessage(status);
        if (body != null)
            response.Content = new StringContent(body, Encoding.UTF8, "application/json");
        if (headers != null)
        {
            foreach (var kv in headers)
            {
                if (!response.Headers.TryAddWithoutValidation(kv.Key, kv.Value))
                    response.Content?.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
            }
        }
        return response;
    }
}
