using System.Net;
using System.Net.Http.Json;
using Bunit;
using BookTracker.Blazor.Api;
using BookTracker.Blazor.Models.Members;
using BookTracker.Blazor.Pages.Members;
using Microsoft.Extensions.DependencyInjection;

namespace BookTracker.Blazor.Tests.Pages.Members;

public class MemberListAndDetailsTests : BunitContext
{
    private sealed class FakeHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => await respond(request);
    }

    private void RegisterClient(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond)
    {
        var httpClient = new HttpClient(new FakeHandler(respond)) { BaseAddress = new Uri("http://localhost") };
        Services.AddSingleton(new BookTrackerClient(httpClient));
    }
    [Fact]
    public void MemberList_ShowsResults_SendsSearch_DisablesNextOnLastPage()
    // One Test >> Proves the list & search & paging together 
    {
        string? capturedUrl = null;

        RegisterClient(request =>
        {
            capturedUrl = request.RequestUri!.ToString();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new GetMemberSummariesResponse
                {
                    Items = [new MemberSummary { Id = 1, Name = "Riyad salem", Email = "riyad1998@bookTracker.com" }],
                    Page = 1,
                    PageSize = 10,
                    TotalItems = 1,
                    TotalPages = 1
                })
            });
        });

        var cut = Render<MemberList>();
        Assert.Contains("Riyad salem", cut.Markup);

        cut.Find("input[type=search]").Input("riy");
        cut.Find("button").Click();
        Assert.Contains("search=riy", capturedUrl);

        var nextButton = cut.FindAll("button").Single(b => b.TextContent.Contains("Volgende"));
        Assert.True(nextButton.HasAttribute("disabled"));
    }

    [Fact]
    public async Task EditMember_LoadsData_ShowsValidationError_ShowsEmailConflict()
    // One Test >> walks the full admin edit flow (loads existing data & shows a 400 validation error & shows the specific 409 email conflict message)
    {
        int callCount = 0;
        HttpStatusCode putStatus = HttpStatusCode.BadRequest;
        object putBody = new { error = "Name is required." };

        RegisterClient(_ =>
        {
            callCount++;
            if (callCount == 1)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(new MemberDetailsResponse { Id = 2, Name = "Mark", Email = "mark@bookTracker.com" })
                });
            }

            return Task.FromResult(new HttpResponseMessage(putStatus) { Content = JsonContent.Create(putBody) });
        });

        var cut = Render<EditMember>(p => p.Add(c => c.Id, 2));
        Assert.Contains("Mark", cut.Markup);

        cut.Find("form").Submit();
        Assert.Contains("Name is required.", cut.Markup); // ValidationError

        putStatus = HttpStatusCode.Conflict;
        cut.Find("form").Submit();
        Assert.Contains("al door een andere gebruiker", cut.Markup); // ConflictError
    }

    [Fact]
    public void DeleteMember_ShowsConfirmation_HandlesAlreadyDeleted()
    // One Test >> proves the full admin delete flow ( confirmation shows the name & a 404 on confirm {someone else already deleted it})
    {
        int callCount = 0;
        RegisterClient(_ =>
        {
            callCount++;
            return Task.FromResult(callCount == 1
                ? new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(new MemberDetailsResponse { Id = 2, Name = "Juan", Email = "juan@bookTracker.com" })
                } : new HttpResponseMessage(HttpStatusCode.NotFound));
        });

        var cut = Render<MemberDetails>(p => p.Add(c => c.Id, 2));

        cut.Find("button").Click();
        Assert.Contains("Juan", cut.Markup);

        cut.Find("button.confirm-delete").Click();
        Assert.Contains("mogelijk al verwijderd", cut.Markup);
    }
}