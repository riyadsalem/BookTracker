using System.Net;
using System.Net.Http.Json;
using BookTracker.Api.Application.Books.GetBookDetails;
using BookTracker.Api.Application.Books.UpdateBook;
using BookTracker.Api.Domain.Books;
using BookTracker.Api.Domain.Members;

namespace BookTracker.Api.IntegrationTests.Books;

[Collection(PostgreSqlCollection.Name)]
public class UpdateBookTests(PostgreSqlFixture database) : IntegrationTest(database)
{
    [Fact]
    public async Task PutBookUpdatesBook()
    {
        await AuthenticateAsMember(MemberRole.Administrator);

        Writer.Seed(db =>
        {
            db.Books.Add(
                new Book
                {
                    Title = new BookTitle("Dune"),
                    Author = new AuthorName("Frank Herbert"),
                    Year = new PublicationYear(1965)
                });
        });

        Guid version = Reader.Query(db =>
            db.Books
                .Where(book => book.Id == 1)
                .Select(book => book.Version)
                .Single());

        UpdateBookRequest request =
            new()
            {
                Title = "Dune Messiah",
                Author = "Frank Herbert",
                Year = 1969,
                Version = version
            };

        var response = await Client.PutAsJsonAsync("/books/1", request);

        await response.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        Book? book = Reader.Query(db => db.Books.Find(1));

        Assert.NotNull(book);
        Assert.Equal("Dune Messiah", book.Title.Value);
        Assert.Equal("Frank Herbert", book.Author.Value);
        Assert.Equal(1969, book.Year.Value);

        Assert.NotEqual(version, book.Version);
    }

    [Fact]
    public async Task PutBookReturnsNotFoundWhenBookDoesNotExist()
    {
        await AuthenticateAsMember(MemberRole.Administrator);

        UpdateBookRequest request =
            new()
            {
                Title = "Unknown Book",
                Author = "Unknown Author",
                Year = 2000,
                Version = Guid.NewGuid()
            };

        var response = await Client.PutAsJsonAsync("/books/9999", request);
        await response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PutBookReturnsBadRequestWhenYearIsOutOfRange()
    {
        await AuthenticateAsMember(MemberRole.Administrator);

        Writer.Seed(db =>
        {
            db.Books.Add(
                new Book
                {
                    Title = new BookTitle("Dune"),
                    Author = new AuthorName("Frank Herbert"),
                    Year = new PublicationYear(1965)
                });
        });

        UpdateBookRequest request =
            new()
            {
                Title = "Dune Messiah",
                Author = "Frank Herbert",
                Year = 99999
            };

        var response = await Client.PutAsJsonAsync("/books/1", request);
        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutBookReturnsConflictForStaleVersion()
    {
        await AuthenticateAsMember(MemberRole.Administrator);

        Writer.Seed(db =>
        {
            db.Books.Add(
                new Book
                {
                    Title = new BookTitle("Dune"),
                    Author = new AuthorName("Frank Herbert"),
                    Year = new PublicationYear(1965)
                });
        });

        var firstResponse = await Client.GetAsync("/books/1");
        GetBookDetailsResponse firstRead = await firstResponse.ReadJsonAs<GetBookDetailsResponse>(HttpStatusCode.OK);

        var secondResponse = await Client.GetAsync("/books/1");
        GetBookDetailsResponse secondRead = await secondResponse.ReadJsonAs<GetBookDetailsResponse>(HttpStatusCode.OK);

        var firstUpdate = new UpdateBookRequest
        {
            Title = "Dune: Special Edition",
            Author = firstRead.Author,
            Year = firstRead.Year,
            Version = firstRead.Version
        };

        var firstUpdateResponse = await Client.PutAsJsonAsync("/books/1", firstUpdate);
        await firstUpdateResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        UpdateBookRequest staleUpdate = new()
        {
            Title = secondRead.Title,
            Author = secondRead.Author,
            Year = 1966,
            Version = secondRead.Version
        };

        var staleUpdateResponse = await Client.PutAsJsonAsync("/books/1", staleUpdate);
        await staleUpdateResponse.ShouldHaveStatusCode(HttpStatusCode.Conflict);

        Book? book = Reader.Query(db => db.Books.Find(1));

        Assert.NotNull(book);
        Assert.Equal("Dune: Special Edition", book.Title.Value);
        Assert.Equal(1965, book.Year.Value);
    }
}