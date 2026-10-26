namespace Datapad.Api.Tests.TestDoubles;

internal sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
{
    private int _requestCount;

    public int RequestCount => _requestCount;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Interlocked.Increment(ref _requestCount);
        return Task.FromResult(respond(request));
    }
}
