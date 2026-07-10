using System.Security.Claims;
using BookTracker.Api.Application.Members;
using BookTracker.Api.Application.Members.CreateMember;
using BookTracker.Api.Application.Members.DeleteMember;
using BookTracker.Api.Application.Members.GetMemberDetails;
using BookTracker.Api.Application.Members.GetMemberSummaries;
using BookTracker.Api.Application.Members.UpdateMember;
using BookTracker.Api.Domain;

namespace BookTracker.Api.Endpoints.Members;

public static class MemberEndpoints
{
    public static IEndpointRouteBuilder MapMemberEndpoints(this IEndpointRouteBuilder app)
    {
        /*
        Anyone can edit and delete any book,
        but only the person edits and deletes themselves....
        no one else can do that.
        */

        app.MapGet("/members", GetMemberSummaries);
        app.MapGet("/members/{id:int}", GetMemberDetails);
        app.MapPost("/members", CreateMember);
        app.MapPut("/members/{id:int}", UpdateMember).RequireAuthorization();
        app.MapDelete("/members/{id:int}", DeleteMember).RequireAuthorization();

        return app;
    }

    private static bool IsCurrentMember(ClaimsPrincipal user, int memberId)
    {
        string? claim = user.FindFirstValue(ClaimTypes.NameIdentifier);

        // TryParse >> "10" -> 10
        return int.TryParse(claim, out var currentMemberId)
            && currentMemberId == memberId;
    }
    public static async Task<IResult> GetMemberSummaries([AsParameters] GetMemberSummariesRequest request, GetMemberSummariesQueryHandler handler) =>
     Results.Ok(await handler.Execute(request));


    public static async Task<IResult> GetMemberDetails(int id, GetMemberDetailsQueryHandler query)
    {
        GetMemberDetailsResponse? member = await query.Execute(id);
        return member is null ? Results.NotFound() : Results.Ok(member);
    }

    public static async Task<IResult> CreateMember(CreateMemberRequest request, CreateMemberCommandHandler handler)
    {
        try
        {
            CreateMemberResponse response = await handler.Execute(request);
            return Results.Created($"/members/{response.Id}", response);
        }
        catch (MemberEmailAlreadyExistsException exception)
        {
            return Results.Conflict(new { error = exception.Message });
        }
        catch (DomainException exception)
        {
            return Results.BadRequest(new { error = exception.Message });
        }
    }
    public static async Task<IResult> UpdateMember(int id, UpdateMemberRequest request, ClaimsPrincipal user, UpdateMemberCommandHandler handler)
    {
        if (!IsCurrentMember(user, id)) return Results.Forbid();

        try
        {
            return await handler.Execute(id, request) ? Results.NoContent() : Results.NotFound();
        }
        catch (MemberEmailAlreadyExistsException exception)
        {
            return Results.Conflict(new { error = exception.Message });
        }
        catch (DomainException exception)
        {
            return Results.BadRequest(new { error = exception.Message });
        }
    }

    public static async Task<IResult> DeleteMember(int id, ClaimsPrincipal user, DeleteMemberCommandHandler handler)
    {
        if (!IsCurrentMember(user, id)) return Results.Forbid();
        return await handler.Execute(id) ? Results.NoContent() : Results.NotFound();
    }

}