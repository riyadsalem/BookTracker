using System.Net;
using System.Net.Http.Json;
using Bunit;
using BookTracker.Blazor.Api;
using BookTracker.Blazor.Models.Books;
using BookTracker.Blazor.Pages.Books;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;

namespace BookTracker.Blazor.Tests.Pages.Books;

public class CreateBookTests : BunitContext
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

    private static void FillForm(IRenderedComponent<CreateBook> cut)
    {
        cut.Find("#title").Change("Leven");
        cut.Find("#author").Change("Riyad Salem");
        cut.Find("#year").Change("2026");
    }

    [Fact]
    public void ValidSubmit_NavigatesTo_BookDetails()
    {
        RegisterClient(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = JsonContent.Create(new CreateBookResponse
            {
                Id = 1,
                Title = "Leven",
                Author = "Riyad Salem",
                Year = 2026
            })
        }));

        var navigationManager = Services.GetRequiredService<NavigationManager>();
        var cut = Render<CreateBook>();

        FillForm(cut);
        cut.Find("form").Submit();

        Assert.EndsWith("/books/1", navigationManager.Uri);
    }

    [Fact]
    public void BadRequest_Shows_ServerMessage()
    {
        RegisterClient(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = JsonContent.Create(new { error = "Title is required." })
        }));

        var cut = Render<CreateBook>();

        FillForm(cut);
        cut.Find("form").Submit();

        Assert.Contains("Title is required.", cut.Markup);
    }

    [Fact]
    public async Task ButtonIsDisabled_WhileSaving()
    {
        var pending = new TaskCompletionSource<HttpResponseMessage>();
        RegisterClient(_ => pending.Task);

        var cut = Render<CreateBook>();
        FillForm(cut);

        var submit = cut.Find("form").SubmitAsync(new EventArgs());

        Assert.True(cut.Find("button[type=submit]").HasAttribute("disabled"));

        pending.SetResult(new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = JsonContent.Create(new CreateBookResponse
            {
                Id = 1,
                Title = "Leven",
                Author = "Riyad Salem",
                Year = 2026
            })
        });

        await submit;
    }
}