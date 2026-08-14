using BookTracker.Blazor.Auth;

namespace BookTracker.Blazor.Tests.Auth;

public class BookTrackerAuthenticationStateProviderTests
{

    private const string ValidToken =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjEiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiUml5YWQiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbmlzdHJhdG9yIiwiZXhwIjo0ODk3MjY4ODAwfQ.fake-signature";
    private const string ExpiredToken =
        "eyJhbGciOiAiSFMyNTYiLCAidHlwIjogIkpXVCJ9.eyJuYW1laWQiOiAiMSIsICJ1bmlxdWVfbmFtZSI6ICJSaXlhZCIsICJleHAiOiAxMDAwMDAwMDAwfQ.fake-signature";

    [Fact]
    public async Task GarbageToken_Means_Anonymous()
    {
        FakeAuthSession session = new();
        await session.SetTokenAsync("JWT...");

        BookTrackerAuthenticationStateProvider provider = new(session);
        var state = await provider.GetAuthenticationStateAsync();

        Assert.False(state.User.Identity?.IsAuthenticated);
    }

    [Fact]
    public async Task ExpiredToken_Means_Anonymous()
    {
        FakeAuthSession session = new();
        await session.SetTokenAsync(ExpiredToken);

        BookTrackerAuthenticationStateProvider provider = new(session);
        var state = await provider.GetAuthenticationStateAsync();

        Assert.False(state.User.Identity?.IsAuthenticated);
    }

    [Fact]
    public async Task NoToken_Means_Anonymous()
    {
        BookTrackerAuthenticationStateProvider provider = new(new FakeAuthSession());
        var state = await provider.GetAuthenticationStateAsync();
        Assert.False(state.User.Identity?.IsAuthenticated);
    }

    [Fact]
    public async Task ValidToken_Becomes_ClaimsPrincipal()
    {
        FakeAuthSession session = new();
        await session.SetTokenAsync(ValidToken);

        BookTrackerAuthenticationStateProvider provider = new(session);
        var state = await provider.GetAuthenticationStateAsync();

        Assert.True(state.User.Identity?.IsAuthenticated);
        Assert.Equal("Riyad", state.User.Identity!.Name);
        Assert.True(state.User.IsInRole("Administrator"));
    }

    [Fact]
    public async Task SignInStoresTheToken()
    {
        FakeAuthSession session = new();
        BookTrackerAuthenticationStateProvider provider = new(session);

        await provider.SignInAsync(ValidToken);

        Assert.Equal(ValidToken, session.StoredToken);
    }

    [Fact]
    public async Task SignOutClearsTheToken()
    {
        FakeAuthSession session = new();
        await session.SetTokenAsync(ValidToken);
        BookTrackerAuthenticationStateProvider provider = new(session);

        await provider.SignOutAsync();

        Assert.Null(session.StoredToken);
    }
}