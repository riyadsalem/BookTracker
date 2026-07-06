using BookTracker.Api.Domain;
using BookTracker.Api.Domain.Members;

namespace BookTracker.Api.Tests.Domain;

public class MemberNameTests
{
    [Fact]
    public void MemberNameAcceptsValidName()
    {
        MemberName author = new MemberName("Riyad");
        Assert.Equal("Riyad", author.Value);
    }

    [Fact]
    public void MemberNameTrimsValue()
    {
        AuthorName author = new AuthorName("  Riyad  ");
        Assert.Equal("Riyad", author.Value);
    }


    [Fact]
    public void MemberNameRejectsWhitespace() =>
    Assert.Throws<DomainException>(() => new MemberName("   "));


    [Fact]
    public void MemberNameRejectsNameLongerThan100Characters() =>
        Assert.Throws<DomainException>(() => new MemberName(new string('x', 101)));

}