using System.Net;
using System.Net.Http.Json;
using BookTracker.Api.Application.UpdateBook;
using BookTracker.Api.Domain;

namespace BookTracker.Api.Tests.IntegrationTests.UpdateBook;

public class UpdateBookTests
{
    private readonly CustomWebApplicationFactory factory = new();

    [Fact]
    public async Task PutBookUpdatesBook()
    {
        EfWriter writer = factory.GetWriter();

        writer.Seed(db =>
        {
            db.Books.Add(
                new Book
                {
                    Title = "Dune",
                    Author = "Frank Herbert",
                    Year = 1965
                });
        });

        UpdateBookRequest request =
            new UpdateBookRequest
            {
                Title = "Dune Messiah",
                Author = "Frank Herbert",
                Year = 1969
            };

        var client = factory.CreateClient();

        var response = await client.PutAsJsonAsync("/books/1", request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        EfReader reader = factory.GetReader();
        Book? book = reader.Query(db => db.Books.Find(1));

        Assert.NotNull(book);
        Assert.Equal("Dune Messiah", book.Title);
        Assert.Equal("Frank Herbert", book.Author);
        Assert.Equal(1969, book.Year);
    }

    [Fact]
    public async Task PutBookReturnsNotFoundWhenBookDoesNotExist()
    {
        UpdateBookRequest request =
            new UpdateBookRequest
            {
                Title = "Unknown Book",
                Author = "Unknown Author",
                Year = 2000
            };

        var client = factory.CreateClient();
        var response = await client.PutAsJsonAsync("/books/9999", request);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}