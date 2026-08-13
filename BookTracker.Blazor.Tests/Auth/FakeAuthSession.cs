using BookTracker.Blazor.Auth;
namespace BookTracker.Blazor.Tests.Auth;

public sealed class FakeAuthSession : IAuthSession
{
    public string? StoredToken { get; private set; }
    public Task<string?> GetTokenAsync() => Task.FromResult(StoredToken);
    public Task SetTokenAsync(string token)
    {
        StoredToken = token;
        return Task.CompletedTask;
    }
    public Task ClearTokenAsync()
    {
        StoredToken = null;
        return Task.CompletedTask;
    }
}