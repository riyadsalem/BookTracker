using System.Net;
using BookTracker.Blazor.Auth;

namespace BookTracker.Blazor.Tests.Auth;

public class AuthorizationMessageHandlerTests
{
    private sealed class CapturingHandler : HttpMessageHandler
    {
        public HttpRequestMessage? CapturedRequest { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CapturedRequest = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }

    [Fact]
    public async Task AddsBearerHeader_When_TokenIsStored()
    {
        FakeAuthSession session = new();
        await session.SetTokenAsync("stored-token");

        var innerHandler = new CapturingHandler();
        AuthorizationMessageHandler handler = new(session) { InnerHandler = innerHandler };

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };

        await httpClient.GetAsync("/books");

        Assert.NotNull(innerHandler.CapturedRequest!.Headers.Authorization);
        Assert.Equal("Bearer", innerHandler.CapturedRequest.Headers.Authorization!.Scheme);
        Assert.Equal("stored-token", innerHandler.CapturedRequest.Headers.Authorization.Parameter);
    }

    [Fact]
    public async Task DoesNot_AddHeader_When_NoTokenIsStored()
    {
        FakeAuthSession session = new();

        var innerHandler = new CapturingHandler();
        AuthorizationMessageHandler handler = new(session) { InnerHandler = innerHandler };

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };

        await httpClient.GetAsync("/books");

        Assert.Null(innerHandler.CapturedRequest!.Headers.Authorization);
    }
}