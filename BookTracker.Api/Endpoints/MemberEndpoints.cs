using System.Security.Claims;
using BookTracker.Api.Application.Members;
using BookTracker.Api.Application.Members.CreateMember;
using BookTracker.Api.Application.Members.DeleteMember;
using BookTracker.Api.Application.Members.GetMemberDetails;
using BookTracker.Api.Application.Members.GetMemberSummaries;
using BookTracker.Api.Application.Members.UpdateMember;
using BookTracker.Api.Domain;
using BookTracker.Api.Domain.Actors;

namespace BookTracker.Api.Endpoints.Members;

public static class MemberEndpoints
{
    public static IEndpointRouteBuilder MapMemberEndpoints(this IEndpointRouteBuilder app)
    {
        /* Anyone can edit and delete any book,
        but only the person edits and deletes themselves....
        no one else can do that.*/

        app.MapGet("/members", GetMemberSummaries).RequireAuthorization();
        app.MapGet("/members/{id:int}", GetMemberDetails).RequireAuthorization();
        app.MapPost("/members", CreateMember);
        app.MapPut("/members/{id:int}", UpdateMember).RequireAuthorization();
        app.MapDelete("/members/{id:int}", DeleteMember).RequireAuthorization();

        return app;
    }

    public static async Task<IResult> GetMemberSummaries([AsParameters] GetMemberSummariesRequest request, ClaimsPrincipal principal, GetMemberSummariesQueryHandler handler)
    {
        try
        {
            Actor actor = principal.ToActor();
            return Results.Ok(await handler.Execute(actor, request));
        }
        catch (ForbiddenOperationException)
        {
            return Results.Forbid();
        }
    }


    public static async Task<IResult> GetMemberDetails(int id, ClaimsPrincipal principal, GetMemberDetailsQueryHandler query)
    {
        try
        {
            Actor actor = principal.ToActor();
            GetMemberDetailsResponse? member = await query.Execute(actor, id);
            return member is null ? Results.NotFound() : Results.Ok(member);
        }
        catch (ForbiddenOperationException)
        {
            return Results.Forbid();
        }
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

    public static async Task<IResult> UpdateMember(int id, UpdateMemberRequest request, ClaimsPrincipal principal, UpdateMemberCommandHandler handler)
    {
        try
        {
            Actor actor = principal.ToActor();

            return await handler.Execute(actor, id, request) ? Results.NoContent() : Results.NotFound();
        }
        catch (ForbiddenOperationException)
        {
            return Results.Forbid();
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

    public static async Task<IResult> DeleteMember(int id, ClaimsPrincipal principal, DeleteMemberCommandHandler handler)
    {
        try
        {
            Actor actor = principal.ToActor();

            return await handler.Execute(actor, id) ? Results.NoContent() : Results.NotFound();
        }
        catch (ForbiddenOperationException)
        {
            return Results.Forbid();
        }
    }

}