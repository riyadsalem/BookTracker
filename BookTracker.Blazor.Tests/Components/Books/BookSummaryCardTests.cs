using Bunit;
using BookTracker.Blazor.Components.Books;
using BookTracker.Blazor.Models.Books;

namespace BookTracker.Blazor.Tests.Components.Books;

public class BookSummaryCardTests : BunitContext
{
    [Fact]
    public void ShowsTitleAndAuthor()
    {
        BookSummary book = new()
        {
            Id = 42,
            Title = "Dune",
            Author = "Frank Herbert"
        };

        // BookSummaryCard.Book = book
        IRenderedComponent<BookSummaryCard> cut = Render<BookSummaryCard>(parameters => parameters.Add(component => component.Book, book));

        Assert.Contains("Dune", cut.Markup);
        Assert.Contains("Frank Herbert", cut.Markup);
    }

    [Fact]
    public void HidesAuthor_When_ShowAuthor_IsFalse()
    {
        BookSummary book = new()
        {
            Id = 10,
            Title = "Leven",
            Author = "Riyad Salem"
        };

        IRenderedComponent<BookSummaryCard> cut = Render<BookSummaryCard>(parameters =>
        parameters.Add(component => component.Book, book).Add(Component => Component.ShowAuthor, false));

        Assert.Contains("Leven", cut.Markup);
        Assert.DoesNotContain("Riyad Salem", cut.Markup);
    }

    [Fact]
    public void ReturnsBookIdWhenSelected()
    {
        BookSummary book = new()
        {
            Id = 42,
            Title = "Dune",
            Author = "Frank Herbert"
        };
        int? selectedBookId = null;

        IRenderedComponent<BookSummaryCard> cut = Render<BookSummaryCard>(parameters => parameters
            .Add(component => component.Book, book)
            .Add(component => component.OnSelected, id => selectedBookId = id));

        cut.Find("article").Click();
        Assert.Equal(42, selectedBookId);
    }

}