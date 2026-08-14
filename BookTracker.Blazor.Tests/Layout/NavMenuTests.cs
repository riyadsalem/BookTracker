using Bunit;
using BookTracker.Blazor.Layout;

namespace BookTracker.Blazor.Tests.Layout;

public class NavMenuTests : BunitContext
{
    [Theory]
    [InlineData(null, "login", "logout")] // zie login ,,,,, not logout
    [InlineData("Member", "logout", "login")] // zie logout ,,,,, not login 
    [InlineData("Administrator", "logout", "login")] // Manager >> Same member,,,, but they will see an additional button as well
    public void Navigation_Shows_CorrectLinks_For_EachUserState(string? role, string expectedLink, string missingLink)
    {
        var authorization = AddAuthorization();

        if (role is null) authorization.SetNotAuthorized();
        else
        {
            authorization.SetAuthorized("Ada Reader");
            authorization.SetRoles(role);
        }

        var cut = Render<NavMenu>();

        Assert.NotEmpty(cut.FindAll($"a[href='{expectedLink}']"));
        Assert.Empty(cut.FindAll($"a[href='{missingLink}']"));
    }

    [Theory]
    [InlineData("Administrator", true)]
    [InlineData("Member", false)]
    public void OnlyAdministrator_Sees_CreateBookAction(string role, bool shouldSeeLink)
    {
        var authorization = AddAuthorization();
        authorization.SetAuthorized("Ada Reader");
        authorization.SetRoles(role);

        var cut = Render<NavMenu>();
        var links = cut.FindAll("a[href='books/create']");

        Assert.Equal(shouldSeeLink, links.Count > 0);
    }
}