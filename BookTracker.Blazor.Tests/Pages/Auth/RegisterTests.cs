using System.Net;
using System.Net.Http.Json;
using Bunit;
using BookTracker.Blazor.Api;
using BookTracker.Blazor.Auth;
using BookTracker.Blazor.Pages.Auth;
using BookTracker.Blazor.Tests.Auth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;

namespace BookTracker.Blazor.Tests.Pages.Auth;

public class RegisterTests : BunitContext
{
    private sealed class FakeHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => await respond(request);
    }

    private FakeAuthSession RegisterClient(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond)
    {
        var httpClient = new HttpClient(new FakeHandler(respond))
        {
            BaseAddress = new Uri("http://localhost")
        };
        Services.AddSingleton(new BookTrackerClient(httpClient));

        var session = new FakeAuthSession();
        Services.AddSingleton<IAuthSession>(session);
        return session;
    }

    private static void FillForm(IRenderedComponent<Register> cut, string password = "riy-123456789", string confirmation = "riy-123456789")
    {
        cut.Find("#name").Change("Riyad Salem");
        cut.Find("#email").Change("riyad1998@bookTracker.com");
        cut.Find("#password").Change(password);
        cut.Find("#passwordConfirmation").Change(confirmation);
    }

    [Fact]
    public void MismatchedPasswords_DoNotSendRequest()
    {
        bool requestSent = false;
        RegisterClient(_ => { requestSent = true; return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)); });

        var cut = Render<Register>();
        FillForm(cut, password: "one-password", confirmation: "another-password");
        cut.Find("form").Submit();

        Assert.False(requestSent);
    }

    [Fact]
    public void ShowsError_For400()
    {
        RegisterClient(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = JsonContent.Create(new { error = "Password must contain at least 8 characters." })
        }));

        var cut = Render<Register>();
        FillForm(cut);
        cut.Find("form").Submit();

        Assert.Contains("Password must contain at least 8 characters.", cut.Markup);
    }

    [Fact]
    public void ShowsExistingAccountMessage_For409()
    {
        RegisterClient(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Conflict)));

        var cut = Render<Register>();
        FillForm(cut);
        cut.Find("form").Submit();

        Assert.Contains("bestaat al een account", cut.Markup); // van api layer
    }

    [Fact]
    public void SuccessNavigates_ToLogin()
    {
        RegisterClient(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Created)));

        var navigationManager = Services.GetRequiredService<NavigationManager>();
        var cut = Render<Register>();
        FillForm(cut);
        cut.Find("form").Submit();

        Assert.EndsWith("/login", navigationManager.Uri);
    }

}