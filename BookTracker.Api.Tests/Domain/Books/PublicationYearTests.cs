using BookTracker.Api.Domain;
using BookTracker.Api.Domain.Books;
namespace BookTracker.Api.Tests.Domain.Books;

public class PublicationYearTests
{
    [Fact]
    public void PublicationYearAcceptsValidYear() =>
    Assert.Equal(1965, new PublicationYear(1965).Value);

    [Fact]
    public void PublicationYearAcceptsMinYear() =>
    Assert.Equal(PublicationYear.MinYear, new PublicationYear(PublicationYear.MinYear).Value);

    [Fact]
    public void PublicationYearRejectsYearBeforeMinYear() =>
    Assert.Throws<DomainException>(() => new PublicationYear(PublicationYear.MinYear - 1));

    [Fact]
    public void PublicationYearAcceptsNextYear()
    {
        int nextYear = DateTime.Now.Year + 1;
        PublicationYear year = new(nextYear);
        Assert.Equal(nextYear, year.Value);
    }

    [Fact]
    public void PublicationYearRejectsTwoYearsAhead() =>
    Assert.Throws<DomainException>(() => new PublicationYear(DateTime.Now.Year + 2));
}