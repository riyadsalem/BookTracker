using System.Net;
using System.Net.Http.Json;
using BookTracker.Api.Application.Books.CreateBook;
using BookTracker.Api.Domain.Books;
using BookTracker.Api.Domain.Members;

namespace BookTracker.Api.Tests.IntegrationTests.Books;

public class CreateBookTests : IntegrationTest
{

    [Fact]
    public async Task PostBookCreatesBook()
    {
        /*
        Creating a book now requires an authenticated member.
        Without this line, the request would be rejected with
        401 before ever reaching CreateBookCommandHandler.
        */
        await AuthenticateAsMember(MemberRole.Administrator);

        CreateBookRequest request = new CreateBookRequest
        {
            Title = "The Heart Is a Lonely Hunter",
            Author = "Carson McCullers",
            Year = 1940
        };

        /*
        zoals >> React -> POST /books
        EN ASP.NET call (bookEndpoints.CreateBook THEN CreateBookCommandHandler THEN EFBookRepository THEN SQLite THEN ((201 Created)))
        */
        var response = await Client.PostAsJsonAsync("/books", request);
        CreateBookResponse? created = await response.ReadJsonAs<CreateBookResponse>(HttpStatusCode.Created);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(created);
        Assert.True(created.Id > 0);
        Assert.Equal("The Heart Is a Lonely Hunter", created.Title);

        Book? book = Reader.Query(context => context.Find<Book>(created!.Id)); // READ >> IS Book actually inside SQLite.... OF Niet

        Assert.NotNull(book);
        Assert.Equal("The Heart Is a Lonely Hunter", book.Title.Value);
        Assert.Equal("Carson McCullers", book.Author.Value);
        Assert.Equal(1940, book.Year);
    }

    [Fact]
    public async Task PostBookReturnsBadRequestWhenTitleIsWhitespace() // TESTS VOOR OBECTVALUE
    {
        await AuthenticateAsMember(MemberRole.Administrator);

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

    [Fact]
    public async Task PostBookReturnsBadRequestWhenYearIsTooLow()
    {
        await AuthenticateAsMember(MemberRole.Administrator);

        CreateBookRequest request = new()
        {
            Title = "Leven",
            Author = "IK",
            Year = -99999
        };

        var response = await Client.PostAsJsonAsync("/books", request);
        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostBookReturnsBadRequestWhenYearIsTooHigh()
    {
        await AuthenticateAsMember(MemberRole.Administrator);

        CreateBookRequest request = new()
        {
            Title = "Toekomst",
            Author = "Jij",
            Year = 99999
        };

        var response = await Client.PostAsJsonAsync("/books", request);
        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

}
