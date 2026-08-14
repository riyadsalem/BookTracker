using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace BookTracker.Blazor.Auth;
// When I want to know the current user, take the JWT from IAuthSession, read the data from it, and trun it into ClaimsPrincipal

// It takes the JWT and converts it into a ClaimsPrincipal object 
// who you are,,,, what your role is,,,, whether you are logged in at all
/*
JWT >> header ... payload ... signature
JWT Token => Split(".") => Payload [1] => Decode Base64URL => JSON => lezen (Id,Name,Email,Role) => zit het in Claims => ClaimsPrincipal => Blazor knows who the user is
*/

public sealed class BookTrackerAuthenticationStateProvider(IAuthSession authSession) : AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync() // START...
    {
        string? token = await authSession.GetTokenAsync();
        ClaimsPrincipal user = CreatePrincipalOrAnonymous(token);

        return new AuthenticationState(user);
    }

    public async Task SignInAsync(string token)
    {
        await authSession.SetTokenAsync(token);

        Console.WriteLine("SIGN IN: Authentication state changed");

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task SignOutAsync()
    {
        await authSession.ClearTokenAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(Anonymous)));
    }

    private static ClaimsPrincipal CreatePrincipalOrAnonymous(string? token)
    // Give me the JWT,,, and I will tell you who the user is ...
    {
        if (string.IsNullOrWhiteSpace(token)) return Anonymous;
        try
        {
            string[] segments = token.Split('.');

            if (segments.Length != 3) return Anonymous;


            byte[] payloadBytes = DecodeBase64Url(segments[1]); // this is (payload...)
            JsonDocument document = JsonDocument.Parse(payloadBytes);
            JsonElement payload = document.RootElement;

            if (!payload.TryGetProperty("exp", out var expiration) ||
                expiration.ValueKind != JsonValueKind.Number ||
                !expiration.TryGetInt64(out var expiresAt) ||
                DateTimeOffset.FromUnixTimeSeconds(expiresAt) <= DateTimeOffset.UtcNow) return Anonymous;

            List<Claim> claims = new();

            // The JWT uses the full ClaimTypes names as property names,
            // so we read the claims directly using ClaimTypes.
            AddClaim(payload, ClaimTypes.NameIdentifier, ClaimTypes.NameIdentifier, claims);
            AddClaim(payload, ClaimTypes.Name, ClaimTypes.Name, claims);
            AddClaim(payload, ClaimTypes.Email, ClaimTypes.Email, claims);
            AddClaim(payload, ClaimTypes.Role, ClaimTypes.Role, claims);

            if (!claims.Exists(claim => claim.Type == ClaimTypes.NameIdentifier)) return Anonymous;


            ClaimsIdentity identity = new(
                claims,
                authenticationType: "jwt",
                nameType: ClaimTypes.Name,
                roleType: ClaimTypes.Role);

            return new ClaimsPrincipal(identity);
        }
        catch (Exception exception) when (exception is FormatException or
                JsonException or ArgumentOutOfRangeException)
        {
            return Anonymous;
        }
    }

    private static void AddClaim(JsonElement payload, string propertyName, string claimType, ICollection<Claim> claims)
    {
        if (!payload.TryGetProperty(propertyName, out var value) ||
            value.ValueKind != JsonValueKind.String) return;

        string? claimValue = value.GetString();

        if (!string.IsNullOrWhiteSpace(claimValue)) claims.Add(new Claim(claimType, claimValue));

    }

    private static byte[] DecodeBase64Url(string value)
    // JWT Payload >> Decode BaseURL >> JSON
    {
        string base64 = value.Replace('-', '+').Replace('_', '/');

        int remainder = base64.Length % 4;

        if (remainder == 2)
        {
            base64 += "==";
        }
        else if (remainder == 3)
        {
            base64 += "=";
        }
        else if (remainder != 0)
        {
            throw new FormatException("Invalid Base64URL payload.");
        }

        return Convert.FromBase64String(base64);
    }
}