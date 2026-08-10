namespace BookTracker.Blazor.Tests.Pages;

using Bunit;
using BookTracker.Blazor.Pages;
public class BookTrackerTests : BunitContext
{
    [Fact]
    public void ShowsAllTemporaryBooks()
    {
        IRenderedComponent<BookTracker> cut = Render<BookTracker>();
        Assert.Contains("Dune", cut.Markup);
        Assert.Contains("Frank Herbert", cut.Markup);
        Assert.Contains("The Big Sleep", cut.Markup);
        Assert.Contains("Raymond Chandler", cut.Markup);
    }

    [Fact]
    public void HidesAuthors_When_ToggleIsClicked()
    {
        IRenderedComponent<BookTracker> cut = Render<BookTracker>();
        cut.Find("button").Click(); // The first one
        Assert.DoesNotContain("Frank Herbert", cut.Markup);
        Assert.DoesNotContain("Raymond Chandler", cut.Markup);
    }
    [Fact]
    public void Shows_SelectedBookId_When_CardIsClicked()
    {
        IRenderedComponent<BookTracker> cut = Render<BookTracker>();
        cut.FindAll("article")[0].Click();
        Assert.Contains("Geselecteerd boek id: 1", cut.Markup);
    }
}