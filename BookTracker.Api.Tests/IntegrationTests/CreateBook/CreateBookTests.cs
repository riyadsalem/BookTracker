using System.Net;
using System.Net.Http.Json;
using BookTracker.Api.Application.CreateBook;
using BookTracker.Api.Domain.Books;

namespace BookTracker.Api.Tests.IntegrationTests.CreateBook;

public class CreateBookTests : IntegrationTest
{

    [Fact]
    public async Task PostBookCreatesBook()
    {
        CreateBookRequest request = new CreateBookRequest
        {
            Title = "The Heart Is a Lonely Hunter",
            Author = "Carson McCullers",
            Year = 1940
        };

        var response = await Client.PostAsJsonAsync("/books", request);
        CreateBookResponse? created = await response.ReadJsonAs<CreateBookResponse>(HttpStatusCode.Created);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(created);
        Assert.True(created.Id > 0);
        Assert.Equal("The Heart Is a Lonely Hunter", created.Title);

        Book? book = Reader.Query(context => context.Find<Book>(created!.Id));

        Assert.NotNull(book);
        Assert.Equal("The Heart Is a Lonely Hunter", book.Title.Value);
        Assert.Equal("Carson McCullers", book.Author.Value);
        Assert.Equal(1940, book.Year);
    }

    [Fact]
    public async Task PostBookReturnsBadRequestWhenTitleIsWhitespace() // TESTS VOOR OBECTVALUE
    {
        CreateBookRequest request = new()
        {
            Title = "   ",
            Author = "Carson McCullers",
            Year = 1940
        };

        var response = await Client.PostAsJsonAsync("/books", request);
        //  Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }
}