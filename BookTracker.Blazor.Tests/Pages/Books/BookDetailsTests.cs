using System.Net;
using System.Net.Http.Json;
using Bunit;
using BookTracker.Blazor.Api;
using BookTracker.Blazor.Models.Books;
using BookTracker.Blazor.Pages.Books;
using Microsoft.Extensions.DependencyInjection;

namespace BookTracker.Blazor.Tests.Pages.Books;

public class BookDetailsTests : BunitContext
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
    }
    private static HttpResponseMessage JsonResponse(BookDetailsResponse body) => new(HttpStatusCode.OK) { Content = JsonContent.Create(body) };

    [Fact]
    public void RequestsBook_By_RouteId()
    {
        string? requestedUrl = null;
        RegisterClient(request =>
        {
            requestedUrl = request.RequestUri?.ToString();
            return Task.FromResult(JsonResponse(new BookDetailsResponse
            {
                Id = 1,
                Title = "Leven",
                Author = "Riyad Salem",
                Year = 2024,
                Version = Guid.NewGuid()
            }));
        });

        Render<BookDetails>(parameters => parameters.Add(component => component.Id, 1));
        Assert.Contains("/books/1", requestedUrl);
    }

    [Fact]
    public void ShowsBook_When_Found()
    {
        RegisterClient(_ => Task.FromResult(JsonResponse(new BookDetailsResponse
        {
            Id = 2,
            Title = "Leven",
            Author = "Riyad Salem",
            Year = 2024,
            Version = Guid.NewGuid()
        })));

        var cut = Render<BookDetails>(parameters => parameters.Add(component => component.Id, 2));
        Assert.Contains("Leven", cut.Markup);
        Assert.Contains("Riyad Salem", cut.Markup);
    }

    [Fact]
    public void Shows_NotFoundState_For404()
    {
        RegisterClient(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)));
        var cut = Render<BookDetails>(parameters => parameters.Add(component => component.Id, 1));
        Assert.Contains("Dit boek bestaat niet.", cut.Markup);
    }

    [Fact]
    public void Reloads_When_IdParameter_Changes()
    // OnParametersSetAsync function in BookDetails page
    {
        RegisterClient(request =>
        {
            string id = request.RequestUri!.ToString().Split('/').Last();
            return Task.FromResult(JsonResponse(new BookDetailsResponse
            {
                Id = int.Parse(id),
                Title = id == "1" ? "Leven" : "Testen",
                Author = id == "1" ? "Riyad Salem" : "Mark",
                Year = 2024,
                Version = Guid.NewGuid()
            }));
        });

        var cut = Render<BookDetails>(parameters => parameters.Add(component => component.Id, 1));
        Assert.Contains("Leven", cut.Markup);

        cut.Render(parameters => parameters.Add(component => component.Id, 2));
        Assert.Contains("Testen", cut.Markup);
        Assert.DoesNotContain("Leven", cut.Markup);
    }
}