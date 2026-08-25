using System.Net;
using System.Net.Http.Json;
using Bunit;
using BookTracker.Blazor.Api;
using BookTracker.Blazor.Models.Books;
using BookTracker.Blazor.Pages.Books;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;

namespace BookTracker.Blazor.Tests.Pages.Books;

public class EditBookTests : BunitContext
{
    private sealed class FakeHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => await respond(request);
    }

    private void RegisterClient(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond)
    {
        var httpClient = new HttpClient(new FakeHandler(respond))
        {
            BaseAddress = new Uri("http://localhost")
        };
        Services.AddSingleton(new BookTrackerClient(httpClient));

        var authorization = AddAuthorization();
        authorization.SetAuthorized("Admin");
        authorization.SetRoles("Administrator");
    }

    private static HttpResponseMessage BookResponse(Guid version) =>
        new(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new BookDetailsResponse
            {
                Id = 1,
                Title = "Leven",
                Author = "Riyad Salem",
                Year = 2026,
                Version = version
            })
        };

    [Fact]
    public void Loads_ExistingBook_IntoForm()
    {
        RegisterClient(_ => Task.FromResult(BookResponse(Guid.NewGuid())));
        var cut = Render<EditBook>(parameters => parameters.Add(c => c.Id, 1));
        Assert.Contains("Leven", cut.Markup);
    }

    [Fact]
    public void Successful_UpdateNavigates_ToDetails()
    {
        int callCount = 0;
        RegisterClient(_ =>
        {
            callCount++;
            return Task.FromResult(callCount == 1 ? BookResponse(Guid.NewGuid()) : new HttpResponseMessage(HttpStatusCode.NoContent));
        });
        var navigationManager = Services.GetRequiredService<NavigationManager>();
        var cut = Render<EditBook>(parameters => parameters.Add(c => c.Id, 1));
        cut.Find("form").Submit();
        Assert.EndsWith("/books/1", navigationManager.Uri);
    }

    [Fact]
    public void ShowsNotFound_When_BookDoesNotExist()
    {
        RegisterClient(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)));
        var cut = Render<EditBook>(parameters => parameters.Add(c => c.Id, 10000));
        Assert.Contains("bestaat niet", cut.Markup);
    }

    [Fact]
    public void Conflict_Shows_ErrorMessage()
    {
        int callCount = 0;
        RegisterClient(_ =>
        {
            callCount++;
            return Task.FromResult(callCount == 1 ? BookResponse(Guid.NewGuid()) : new HttpResponseMessage(HttpStatusCode.Conflict)
            {
                Content = JsonContent.Create(new { error = "The book was changed by another user." })
            });
        });
        var cut = Render<EditBook>(parameters => parameters.Add(c => c.Id, 1));
        cut.Find("form").Submit();
        Assert.Contains("The book was changed by another user.", cut.Markup);
    }
}