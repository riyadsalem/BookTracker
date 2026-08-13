using System.Net;
using System.Net.Http.Json;
using Bunit;
using BookTracker.Blazor.Api;
using BookTracker.Blazor.Auth;
using BookTracker.Blazor.Models.Auth;
using BookTracker.Blazor.Pages.Auth;
using BookTracker.Blazor.Tests.Auth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;

namespace BookTracker.Blazor.Tests.Pages.Auth;

public class LoginTests : BunitContext
{
    private sealed class FakeHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => await respond(request);
    }

    private FakeAuthSession RegisterClientAndSession(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond)
    {
        var httpClient = new HttpClient(new FakeHandler(respond))
        {
            BaseAddress = new Uri("http://localhost")
        };
        Services.AddSingleton(new BookTrackerClient(httpClient));

        FakeAuthSession fakeSession = new();
        Services.AddSingleton<IAuthSession>(fakeSession);

        return fakeSession;
    }

    [Fact]
    public void SubmittingForm_Calls_LoginWithEnteredCredentials()
    // (Email & Password) > Submit > Login() > BookTrackerClient.Login(model) > POST /auth/login
    {
        HttpRequestMessage? capturedRequest = null;

        FakeAuthSession session = RegisterClientAndSession(async request =>
        // Fake Client & Fake Session
        {
            capturedRequest = request;
            LoginResponse body = new() // Fake Response
            {
                AccessToken = "fake-token",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(body)
            };
        });

        var cut = Render<Login>();

        cut.Find("#email").Change("ada@bookTracker.com");
        cut.Find("#password").Change("Riyad123123");
        cut.Find("form").Submit();

        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Post, capturedRequest!.Method);
        Assert.Contains("/auth/login", capturedRequest.RequestUri!.ToString());
        Assert.Equal("fake-token", session.StoredToken);
    }

    [Fact]
    public void InvalidForm_DoesNotSend_LoginRequest()
    {
        bool requestSent = false;

        RegisterClientAndSession(_ =>
        {
            requestSent = true;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        var cut = Render<Login>();

        cut.Find("#password").Change("Mark...123");
        cut.Find("form").Submit();

        Assert.False(requestSent);
    }

    [Fact]
    public void ShowsError_For_InvalidCredentials()
    {
        RegisterClientAndSession(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized)));

        var cut = Render<Login>();

        cut.Find("#email").Change("ada@bookTracker.com");
        cut.Find("#password").Change("wrong-password");
        cut.Find("form").Submit();

        Assert.Contains("E-mail of wachtwoord is onjuist.", cut.Markup);
    }

    [Fact]
    public void SuccessfulLoginStoresToken_And_NavigatesAway()
    {
        FakeAuthSession session = RegisterClientAndSession(_ =>
        {
            LoginResponse body = new()
            {
                AccessToken = "fake-token",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(body)
            });
        });

        var navigationManager = Services.GetRequiredService<NavigationManager>();

        var cut = Render<Login>();

        cut.Find("#email").Change("ada@bookTracker.com");
        cut.Find("#password").Change("Riyad525252");
        cut.Find("form").Submit();

        Assert.Equal("fake-token", session.StoredToken);
        Assert.EndsWith("/booktracker", navigationManager.Uri);
    }
}