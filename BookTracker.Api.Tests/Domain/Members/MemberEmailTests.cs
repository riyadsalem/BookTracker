using BookTracker.Api.Domain;
using BookTracker.Api.Domain.Members;

namespace BookTracker.Api.Tests.Domain;

public class MemberEmailTests
{
    [Fact]
    public void MemberEmailAcceptsValidEmail()
    {
        MemberEmail email = new MemberEmail("riyad.m.salem.19988@gmail.com");
        Assert.Equal("riyad.m.salem.19988@gmail.com", email.Value);
    }

    [Fact]
    public void MemberEmailTrimsValue()
    {
        MemberEmail email = new MemberEmail("  riyad.m.salem.19988@gmail.com  ");
        Assert.Equal("riyad.m.salem.19988@gmail.com", email.Value);
    }


    [Fact]
    public void MemberEmailRejectsWhitespace() =>
    Assert.Throws<DomainException>(() => new MemberEmail("   "));


    [Fact]
    public void MemberEmailRejectsEmailLongerThan200Characters() =>
    Assert.Throws<DomainException>(() => new MemberEmail(new string('x', 201)));

    [Fact]
    public void MemberEmailThrowsWhenMissingAt() =>
        Assert.Throws<DomainException>(() => new MemberEmail("r.m.s.gamil.com"));

    [Fact]
    public void MemberEmailRejectsNull() =>
    Assert.Throws<DomainException>(() => new MemberEmail(null!));

}