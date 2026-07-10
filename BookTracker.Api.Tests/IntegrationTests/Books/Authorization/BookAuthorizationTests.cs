using System.Net;
using System.Net.Http.Json;
using BookTracker.Api.Application.Books.CreateBook;
using BookTracker.Api.Application.Books.UpdateBook;
using BookTracker.Api.Domain.Books;

namespace BookTracker.Api.Tests.IntegrationTests.Books.Authorization;

public class BookAuthorizationTests : IntegrationTest
{
    [Fact]
    public async Task CreateBookRequiresAuthentication()
    {
        var request =
            new CreateBookRequest
            {
                Title = "Dune",
                Author = "Frank Herbert",
                Year = 1965
            };

        var response =
            await Client.PostAsJsonAsync("/books", request);

        await response.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);

        var count = Reader.Query(db => db.Books.Count());
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task UpdateBookRequiresAuthentication()
    {
        Writer.Seed(db =>
        {
            db.Books.Add(new Book
            {
                Title = new BookTitle("Dune"),
                Author = new AuthorName("Frank Herbert"),
                Year = new PublicationYear(1965)
            });
        });

        var request =
            new UpdateBookRequest
            {
                Title = "Changed Title",
                Author = "Changed Author",
                Year = 2000
            };

        var response =
            await Client.PutAsJsonAsync("/books/1", request);

        await response.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);

        Book? book = Reader.Query(db => db.Books.Find(1));
        Assert.NotNull(book);
        Assert.Equal("Dune", book.Title.Value); // Still the original title - the update never happened.

    }

    [Fact]
    public async Task DeleteBookRequiresAuthentication()
    {
        Writer.Seed(db =>
        {
            db.Books.Add(new Book
            {
                Id = 1,
                Title = new BookTitle("Dune"),
                Author = new AuthorName("Frank Herbert"),
                Year = new PublicationYear(1965)
            });
        });

        var response = await Client.DeleteAsync("/books/1");

        await response.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);

        Book? book = Reader.Query(db => db.Books.Find(1));
        Assert.NotNull(book);
    }

    [Fact]
    public async Task GetBooksDoesNotRequireAuthentication()
    {
        var response = await Client.GetAsync("/books");
        await response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetBookByIdDoesNotRequireAuthentication()
    {
        Writer.Seed(db =>
        {
            db.Books.Add(new Book
            {
                Id = 1,
                Title = new BookTitle("Dune"),
                Author = new AuthorName("Frank Herbert"),
                Year = new PublicationYear(1965)
            });
        });

        var response = await Client.GetAsync("/books/1");
        await response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RegularMemberCannotCreateBook()
    {
        await AuthenticateAsMember();

        CreateBookRequest request = new()
        {
            Title = "Dune",
            Author = "Frank Herbert",
            Year = 1965
        };

        var response =
            await Client.PostAsJsonAsync("/books", request);

        await response.ShouldHaveStatusCode(HttpStatusCode.Forbidden);

        int count = Reader.Query(db => db.Books.Count());
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task RegularMemberCannotUpdateeBook()
    {
        await AuthenticateAsMember();

        Writer.Seed(db =>
        {
            db.Books.Add(new Book
            {
                Title = new BookTitle("Dune"),
                Author = new AuthorName("Frank Herbert"),
                Year = new PublicationYear(1965)
            });
        });

        CreateBookRequest request = new()
        {
            Title = "III",
            Author = "Riyad",
            Year = 2026
        };

        var response = await Client.PutAsJsonAsync("/books/1", request);

        await response.ShouldHaveStatusCode(HttpStatusCode.Forbidden);

        Book? book = Reader.Query(db => db.Books.Find(1));
        Assert.NotNull(book);
        Assert.Equal("Dune", book.Title.Value);
    }

    [Fact]
    public async Task RegularMemberCannotDeleteBook()
    {
        await AuthenticateAsMember();

        Writer.Seed(db =>
        {
            db.Books.Add(new Book
            {
                Id = 1,
                Title = new BookTitle("Dune"),
                Author = new AuthorName("Frank Herbert"),
                Year = new PublicationYear(1965)
            });
        });

        var response = await Client.DeleteAsync("/books/1");

        await response.ShouldHaveStatusCode(HttpStatusCode.Forbidden);

        Book? book = Reader.Query(db => db.Books.Find(1));
        Assert.NotNull(book);
    }
}