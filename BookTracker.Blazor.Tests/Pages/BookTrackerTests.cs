namespace BookTracker.Blazor.Tests.Pages;

using System.Net;
using System.Net.Http.Json;
using Bunit;
using BookTracker.Blazor.Api;
using BookTracker.Blazor.Models.Books;
using BookTracker.Blazor.Pages;
using Microsoft.Extensions.DependencyInjection;
public class BookTrackerTests : BunitContext
{
    private sealed class FakeHandler(Task<HttpResponseMessage> response) : HttpMessageHandler
    // Normaal >> HttpClient => Internet => BookTracker.Api
    // Hier >> gebruik ik FakeHandler (Request >>> (Zonder internet)[FakeHandler] >>> Response)
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => await response;
    }

    private void RegisterClient(Task<HttpResponseMessage> response)
    // prepere BookTrackerClient voor Testen
    {
        var httpClient = new HttpClient(new FakeHandler(response))
        {
            BaseAddress = new Uri("http://localhost")
        }; // Create HttpClient Bij FakeHandler...
        Services.AddSingleton(new BookTrackerClient(httpClient));
    }

    private static HttpResponseMessage JsonResponse(GetBookSummariesResponse body) => new(HttpStatusCode.OK) { Content = JsonContent.Create(body) };
    // Maak response zoals (Api Response)
    /*
    HttP 200 ok 
    Content-Type: application/json
    { "items": [....], ...}
    */

    private static GetBookSummariesResponse EmptyResult() => new()
    // Api weerk maar hij vind geen books 
    {
        Items = [],
        Page = 1,
        PageSize = 10,
        TotalItems = 0,
        TotalPages = 0
    };

    [Fact]
    public void ShowsBooks_ReturnedBy_TheApi()
    {
        RegisterClient(Task.FromResult(JsonResponse(new GetBookSummariesResponse
        {
            Items = [new BookSummary { Id = 1, Title = "Leven", Author = "Riyad Salem" }],
            Page = 1,
            PageSize = 10,
            TotalItems = 1,
            TotalPages = 1
        })));

        var cut = Render<BookTracker>();

        Assert.Contains("Leven", cut.Markup);
        Assert.Contains("Riyad Salem", cut.Markup);
    }

    [Fact]
    public void ShowsLoadingState_While_RequestIsInFlight()
    {
        var completionSource = new TaskCompletionSource<HttpResponseMessage>();
        RegisterClient(completionSource.Task);

        var cut = Render<BookTracker>();

        Assert.Contains("Boeken laden...", cut.Markup);
        completionSource.SetResult(JsonResponse(EmptyResult()));
    }

    [Fact]
    public void ShowsEmptyState_When_NoBooksAreFound()
    {
        RegisterClient(Task.FromResult(JsonResponse(EmptyResult())));

        var cut = Render<BookTracker>();

        Assert.Contains("Geen boeken gevonden.", cut.Markup);
    }

    [Fact]
    public void ShowsErrorState_When_ApiIsUnreachable()
    {
        RegisterClient(Task.FromException<HttpResponseMessage>(new HttpRequestException("Connection refused")));

        var cut = Render<BookTracker>();

        Assert.Contains("Boeken konden niet worden geladen", cut.Markup);
        Assert.DoesNotContain("Connection refused", cut.Markup);
    }

    [Fact]
    public void PagingButtons_AreDisabled_OnTheOnlyPage()
    {
        RegisterClient(Task.FromResult(JsonResponse(new GetBookSummariesResponse
        {
            Items = [new BookSummary { Id = 1, Title = "Leven", Author = "Riyad Salem" }],
            Page = 1,
            PageSize = 10,
            TotalItems = 1,
            TotalPages = 1
        })));

        var cut = Render<BookTracker>();

        var buttons = cut.FindAll("button");
        var previousButton = buttons.Single(b => b.TextContent.Contains("Vorige"));
        var nextButton = buttons.Single(b => b.TextContent.Contains("Volgende"));

        Assert.True(previousButton.HasAttribute("disabled"));
        Assert.True(nextButton.HasAttribute("disabled"));
    }
}