using System.Net;
using System.Net.Http.Json;
using Bunit;
using BookTracker.Blazor.Api;
using BookTracker.Blazor.Auth;
using BookTracker.Blazor.Models.Members;
using BookTracker.Blazor.Tests.Auth;
using BookTracker.Blazor.Pages.Members;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;


namespace BookTracker.Blazor.Tests.Pages.Account;

public class SelfServiceTests : BunitContext
{
    private sealed class FakeHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => await respond(request);
    }

    private static HttpResponseMessage MeResponse() => new(HttpStatusCode.OK)
    {
        Content = JsonContent.Create(new CurrentMemberResponse { Id = 1, Name = "ٌRiyad", Email = "Riyad1998@bookTracker.com", Role = "Member" })
    };

    private BookTrackerAuthenticationStateProvider RegisterClientAndAuth(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond)
    {
        HttpClient httpClient = new(new FakeHandler(respond)) { BaseAddress = new Uri("http://localhost") };
        Services.AddSingleton(new BookTrackerClient(httpClient));

        FakeAuthSession session = new();
        BookTrackerAuthenticationStateProvider provider = new(session);
        Services.AddSingleton(provider);
        Services.AddSingleton<AuthenticationStateProvider>(provider);

        return provider;
    }

    [Fact]
    public async Task ViewAndEditOwnAccount_UsesAuthMe_ShowsDataAndUpdateMessage()
    // One test >> Proves self service use (/auth/me) & that a self edit succeeds with the doumneted ((Log in again)) message
    {
        List<string> requestedPaths = new();
        int callCount = 0;

        RegisterClientAndAuth(request =>
        {
            requestedPaths.Add(request.RequestUri!.AbsolutePath);
            callCount++;
            return Task.FromResult(callCount == 1 ? MeResponse() : new HttpResponseMessage(HttpStatusCode.NoContent));
        });

        var accountCut = Render<MyAccount>();
        Assert.Contains("Riyad", accountCut.Markup);
        Assert.Contains("/auth/me", requestedPaths);
        Assert.DoesNotContain("/members/1", requestedPaths);

        callCount = 0; // reset voor de EditAccount pagina
        var editCut = Render<EditAccount>();
        editCut.Find("form").Submit();

        Assert.Contains("Log opnieuw in", editCut.Markup);
    }

    [Fact]
    public async Task DeleteOwnAccount_SignsOutAndNavigatesToPublicPage()
    // One test >> proves self delete performs the full cleanup (signs out and navigates to a public page)
    {
        int callCount = 0;
        var provider = RegisterClientAndAuth(_ =>
        {
            callCount++;
            return Task.FromResult(callCount == 1 ? MeResponse() : new HttpResponseMessage(HttpStatusCode.NoContent));
        });

        await provider.SignInAsync("some-token"); // simuleer een ingelogde gebruiker

        NavigationManager navigationManager = Services.GetRequiredService<NavigationManager>();
        var cut = Render<MyAccount>();

        cut.Find("button").Click();
        cut.FindAll("button")[0].Click();

        Assert.EndsWith("/booktracker", navigationManager.Uri);
    }
}