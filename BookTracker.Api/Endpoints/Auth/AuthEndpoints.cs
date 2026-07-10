using System.Security.Claims;
using BookTracker.Api.Application.Auth.GetCurrentMember;
using BookTracker.Api.Application.Auth.Login;

namespace BookTracker.Api.Endpoints.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login", Login);
        app.MapGet("/auth/me", GetCurrentMember).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> Login(LoginRequest request, LoginCommandHandler handler)
    {
        LoginResponse? response = await handler.Execute(request);
        return response is null ? Results.Unauthorized() // 401
         : Results.Ok(response);
    }

    private static IResult GetCurrentMember(ClaimsPrincipal user)
    // ClaimsPrincipal >>> Current User 
    /*
    ASP.NET leest dat automatisch
    ClaimsPrincipal {
    Claims = [ Claim { Type = "NameIdentifier", Value = "7" }, Claim { Type = "Name", Value = "Riyad"},]
    }
    */
    {
        string id = user.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        string name = user.FindFirst(ClaimTypes.Name)!.Value;
        string email = user.FindFirst(ClaimTypes.Email)!.Value;
        string role = user.FindFirst(ClaimTypes.Role)!.Value;

        return Results.Ok(new CurrentMemberResponse
        {
            Id = int.Parse(id),
            Name = name,
            Email = email,
            Role = role
        });
    }
}