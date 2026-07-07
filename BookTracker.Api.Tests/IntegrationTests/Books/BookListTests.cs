using System.Net;
using System.Net.Http.Json;
using BookTracker.Api.Application.Books.GetBookSummaries;
using BookTracker.Api.Application.GetBookSummaries;
using BookTracker.Api.Domain.Books;

namespace BookTracker.Api.Tests.IntegrationTests.Books;

public class BookListTests : IntegrationTest
{
    [Fact]
    public async Task GetBookSummariesReturnsBookSummaries()
    {
        Writer.Seed(db => db.Books.Add(
            new Book
            {
                Title = new BookTitle("Cannery Row"),
                Author = new AuthorName("John Steinbeck"),
                Year = 1945
            }
        ));


        var response = await Client.GetAsync("/books");
        // var result = await Client.GetFromJsonAsync<PagedResult<BookSummary>>("/books");

        PagedResult<BookSummary> result = await response.ReadJsonAs<PagedResult<BookSummary>>(HttpStatusCode.OK); // Als de status Correct is 

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);

        BookSummary Booksummary = Assert.Single(result.Items);

        Assert.Equal("Cannery Row", Booksummary.Title);
        Assert.Equal("John Steinbeck", Booksummary.Author);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(1, result.TotalItems);
        Assert.Equal(1, result.TotalPages);
    }

    [Fact]
    public async Task GetBooksReturnsRequestedPage()
    {
        /*
        For example.. I want to test ::: But the DB is empty ((How would i GETBOOKS that doesn't exist??))
        SO... before testing, I ADDBOOKS
        */
        // Prepare the environment.. (Arrange)
        Writer.Seed(db =>
        {
            db.Books.AddRange(
                new Book
                {
                    Title = new BookTitle("Book 1"),
                    Author = new AuthorName("Author 1"),
                    Year = 2001
                },
                new Book
                {
                    Title = new BookTitle("Book 2"),
                    Author = new AuthorName("Author 2"),
                    Year = 2002
                },
                new Book
                {
                    Title = new BookTitle("Book 3"),
                    Author = new AuthorName("Author 3"),
                    Year = 2003
                });
        });

        // EXECUTE (Act)
        PagedResult<BookSummary>? result = await Client.GetFromJsonAsync<PagedResult<BookSummary>>("/books?page=2&pageSize=1");

        // Assert
        Assert.NotNull(result);

        BookSummary book = Assert.Single(result.Items);

        Assert.Equal("Book 2", book.Title);
        Assert.Equal(2, result.Page);
        Assert.Equal(1, result.PageSize);
        Assert.Equal(3, result.TotalItems);
        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public async Task GetBooksReturnsEmptyItemsWhenPageIsTooHigh()
    {
        Writer.Seed(db =>
        {
            db.Books.Add(
                new Book
                {
                    Title = new BookTitle("Book 1"),
                    Author = new AuthorName("Author 1"),
                    Year = 2001
                });
        });

        PagedResult<BookSummary>? result = await Client.GetFromJsonAsync<PagedResult<BookSummary>>("/books?page=99&pageSize=10");

        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(99, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(1, result.TotalItems);
        Assert.Equal(1, result.TotalPages);
    }

    [Fact]
    public async Task GetBooksCanSearchByTitle()
    {
        Writer.Seed(db =>
        {
            db.Books.AddRange(
                new Book
                {
                    Title = new BookTitle("Dune"),
                    Author = new AuthorName("Frank Herbert"),
                    Year = 1965
                },
                new Book
                {
                    Title = new BookTitle("The Big Sleep"),
                    Author = new AuthorName("Raymond Chandler"),
                    Year = 1939
                });
        });

        var response = await Client.GetAsync("/books?search=dune");
        PagedResult<BookSummary> result = await response.ReadJsonAs<PagedResult<BookSummary>>(HttpStatusCode.OK);
        BookSummary book = Assert.Single(result.Items);

        Assert.Equal("Dune", book.Title);
        Assert.Equal("Frank Herbert", book.Author);
        Assert.Equal(1, result.TotalItems);
        Assert.Equal(1, result.TotalPages);
    }

    [Fact]
    public async Task GetBooksCanSearchByAuthor()
    {
        Writer.Seed(db =>
        {
            db.Books.AddRange(
                new Book
                {
                    Title = new BookTitle("Dune"),
                    Author = new AuthorName("Frank Herbert"),
                    Year = 1965
                },
                new Book
                {
                    Title = new BookTitle("The Big Sleep"),
                    Author = new AuthorName("Raymond Chandler"),
                    Year = 1939
                });
        });

        var response = await Client.GetAsync("/books?search=frank");
        PagedResult<BookSummary> result = await response.ReadJsonAs<PagedResult<BookSummary>>(HttpStatusCode.OK);
        BookSummary book = Assert.Single(result.Items);

        Assert.Equal("Dune", book.Title);
        Assert.Equal("Frank Herbert", book.Author);
        Assert.Equal(1, result.TotalItems);
        Assert.Equal(1, result.TotalPages);
    }

    [Fact]
    public async Task GetBooksAppliesPagingAfterSearch()
    {
        Writer.Seed(db =>
        {
            db.Books.AddRange(
                new Book
                {
                    Title = new BookTitle("Dune"),
                    Author = new AuthorName("Frank Herbert"),
                    Year = 1965
                },
                new Book
                {
                    Title = new BookTitle("Dune Messiah"),
                    Author = new AuthorName("Frank Herbert"),
                    Year = 1969
                },
                new Book
                {
                    Title = new BookTitle("The Big Sleep"),
                    Author = new AuthorName("Raymond Chandler"),
                    Year = 1939
                });
        });

        var response = await Client.GetAsync("/books?search=dune&page=2&pageSize=1");
        PagedResult<BookSummary> result = await response.ReadJsonAs<PagedResult<BookSummary>>(HttpStatusCode.OK);
        BookSummary book = Assert.Single(result.Items);

        Assert.Equal("Dune Messiah", book.Title);
        Assert.Equal(2, result.Page);
        Assert.Equal(1, result.PageSize);
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public async Task GetBooksReturnsEmptyWhenSearchHasNoMatches()
    {
        Writer.Seed(db =>
        {
            db.Books.AddRange(
                new Book
                {
                    Title = new BookTitle("Dune"),
                    Author = new AuthorName("Frank Herbert"),
                    Year = 1965
                },
                new Book
                {
                    Title = new BookTitle("The Big Sleep"),
                    Author = new AuthorName("Raymond Chandler"),
                    Year = 1939
                });
        });

        var response = await Client.GetAsync("/books?search=java");
        PagedResult<BookSummary> result = await response.ReadJsonAs<PagedResult<BookSummary>>(HttpStatusCode.OK);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalItems);
        Assert.Equal(0, result.TotalPages);
    }

}

