using System.Security.Claims;
using BookTracker.Api.Domain.Actors;
using BookTracker.Api.Domain.Members;

namespace BookTracker.Api.Endpoints;

public static class ClaimsPrincipalExtensions
{
    public static Actor ToActor(this ClaimsPrincipal principal) // ClaimsPrincipal principal >>> JWT 
    // ClaimsPrincipal [{name: NameIdentifier, value: "5"}....]
    {
        string? memberIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        string? roleValue = principal.FindFirstValue(ClaimTypes.Role);

        if (!int.TryParse(memberIdValue, out int memberId))
            throw new InvalidOperationException("Authenticated user has no valid member id.");


        if (!Enum.TryParse<MemberRole>(roleValue, out MemberRole role))
            throw new InvalidOperationException("Authenticated user has no valid member role.");

        return new Actor(memberId, role);
    }
}