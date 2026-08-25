using System.Net;
using System.Net.Http.Json;
using Bunit;
using BookTracker.Blazor.Api;
using BookTracker.Blazor.Models.Books;
using BookTracker.Blazor.Pages.Books;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;

namespace BookTracker.Blazor.Tests.Pages.Books;

public class BookDetailsDeleteTests : BunitContext
{
    private HttpResponseMessage BookJson() => new(HttpStatusCode.OK)
    {
        Content = JsonContent.Create(new BookDetailsResponse
        {
            Id = 1,
            Title = "Leven",
            Author = "Riyad Salem",
            Year = 2026,
            Version = Guid.NewGuid()
        })
    };

    private void SetupAs(string role, HttpResponseMessage deleteResponse)
    {
        int callCount = 0;
        var handler = new DelegatingFake(request =>
        {
            callCount++;
            return Task.FromResult(callCount == 1 ? BookJson() : deleteResponse);
        });

        Services.AddSingleton(new BookTrackerClient(new HttpClient(handler) { BaseAddress = new Uri("http://localhost") }));

        var authorization = AddAuthorization();
        authorization.SetAuthorized("Test");
        authorization.SetRoles(role);
    }

    private sealed class DelegatingFake(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken c) => await respond(r);
    }

    [Fact]
    public void OnlyAdministrator_SeesDeleteButton()
    {
        SetupAs("Member", new HttpResponseMessage(HttpStatusCode.NoContent));
        var cut = Render<BookDetails>(p => p.Add(c => c.Id, 1));
        Assert.DoesNotContain("Verwijderen", cut.Markup);
    }

    [Fact]
    public void ClickingDelete_Asks_ForConfirmationWithTitle()
    {
        SetupAs("Administrator", new HttpResponseMessage(HttpStatusCode.NoContent));
        var cut = Render<BookDetails>(p => p.Add(c => c.Id, 1));
        cut.Find("button").Click();
        Assert.Contains("Leven", cut.Markup);
    }

    [Fact]
    public void SuccessfulDelete_GoesBackToList()
    {
        SetupAs("Administrator", new HttpResponseMessage(HttpStatusCode.NoContent));
        var nav = Services.GetRequiredService<NavigationManager>();
        var cut = Render<BookDetails>(p => p.Add(c => c.Id, 1));

        cut.Find("button").Click();
        cut.Find("button.confirm-delete").Click();

        Assert.EndsWith("/booktracker", nav.Uri);
    }

    [Fact]
    public void Deleting_AlreadyDeletedBook_ShowsFriendlyMessage()
    {
        SetupAs("Administrator", new HttpResponseMessage(HttpStatusCode.NotFound));
        var cut = Render<BookDetails>(p => p.Add(c => c.Id, 1));

        cut.Find("button").Click();
        cut.Find("button.confirm-delete").Click();

        Assert.Contains("mogelijk al verwijderd", cut.Markup);
    }
}