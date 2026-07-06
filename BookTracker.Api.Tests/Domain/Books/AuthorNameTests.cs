using BookTracker.Api.Domain;
using BookTracker.Api.Domain.Books;

namespace BookTracker.Api.Tests.Domain;

public class AuthorNameTests
{
    [Fact]
    public void AuthorNameAcceptsValidName()
    {
        AuthorName author = new AuthorName("F. Scott Fitzgerald");

        Assert.Equal("F. Scott Fitzgerald", author.Value);
    }

    [Fact]
    public void AuthorNameTrimsValue()
    {
        AuthorName author = new AuthorName("  Frank Herbert  ");
        Assert.Equal("Frank Herbert", author.Value);
    }


    [Fact]
    public void AuthorNameRejectsWhitespace()
    {
        DomainException exception = Assert.Throws<DomainException>(() => new AuthorName("   "));
        Assert.Equal("Author is required.", exception.Message);
    }

    [Fact]
    public void AuthorNameRejectsNameLongerThan100Characters()
    {
        string tooLong = new string('x', 101);
        var exception = Assert.Throws<DomainException>(() => new AuthorName(tooLong));
        Assert.Equal("Author cannot be longer than 100 characters.", exception.Message);
    }
}