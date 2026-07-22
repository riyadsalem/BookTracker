using System.Security.Claims;
using BookTracker.Api.Application.Members.CreateMember;
using BookTracker.Api.Application.Members.DeleteMember;
using BookTracker.Api.Application.Members.GetMemberDetails;
using BookTracker.Api.Application.Members.GetMemberSummaries;
using BookTracker.Api.Application.Members.UpdateMember;

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

    public static async Task<IResult> GetMemberSummaries([AsParameters] GetMemberSummariesRequest request, ClaimsPrincipal principal, GetMemberSummariesQueryHandler handler) =>
    Results.Ok(await handler.Execute(principal.ToActor(), request));


    public static async Task<IResult> GetMemberDetails(int id, ClaimsPrincipal principal, GetMemberDetailsQueryHandler query)
    {
        GetMemberDetailsResponse? member = await query.Execute(principal.ToActor(), id);
        return member is null ? Results.NotFound() : Results.Ok(member);
    }

    public static async Task<IResult> CreateMember(CreateMemberRequest request, CreateMemberCommandHandler handler)
    {
        CreateMemberResponse member = await handler.Execute(request);
        return Results.Created($"/members/{member.Id}", member);
    }

    public static async Task<IResult> UpdateMember(int id, UpdateMemberRequest request, ClaimsPrincipal principal, UpdateMemberCommandHandler handler) =>
    await handler.Execute(principal.ToActor(), id, request) ? Results.NoContent() : Results.NotFound();

    public static async Task<IResult> DeleteMember(int id, ClaimsPrincipal principal, DeleteMemberCommandHandler handler) =>
    await handler.Execute(principal.ToActor(), id) ? Results.NoContent() : Results.NotFound();


}