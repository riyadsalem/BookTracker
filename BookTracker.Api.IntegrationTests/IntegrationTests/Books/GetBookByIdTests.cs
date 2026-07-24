using System.Net;
using BookTracker.Api.Application.Books.GetBookDetails;
using BookTracker.Api.Domain.Books;

namespace BookTracker.Api.IntegrationTests.Books;

[Collection(PostgreSqlCollection.Name)]
public class GetBookByIdTests(PostgreSqlFixture database) : IntegrationTest(database)
{
    [Fact]
    public async Task GetBookDetailsReturnsBookDetail()
    {
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

        var response = await Client.GetAsync("/books/1");
        GetBookDetailsResponse? book = await response.ReadJsonAs<GetBookDetailsResponse>(HttpStatusCode.OK);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(book);
        Assert.Equal(1, book.Id);
        Assert.Equal("Dune", book.Title);
        Assert.Equal("Frank Herbert", book.Author);
        Assert.Equal(1965, book.Year);
    }

    [Fact]
    public async Task GetBookByIdReturnsNotFoundWhenBookDoesNotExist()
    {
        var response = await Client.GetAsync("/books/9999");
        // Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await response.ShouldHaveStatusCode(HttpStatusCode.NotFound);

    }
}

