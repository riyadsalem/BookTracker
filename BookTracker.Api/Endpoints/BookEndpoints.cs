using System.Security.Claims;
using BookTracker.Api.Application.Books.CreateBook;
using BookTracker.Api.Application.Books.DeleteBook;
using BookTracker.Api.Application.Books.GetBookDetails;
using BookTracker.Api.Application.Books.GetBookSummaries;
using BookTracker.Api.Application.Books.UpdateBook;
using BookTracker.Api.Domain;
using BookTracker.Api.Domain.Actors;
using BookTracker.Api.Storage.Books;

namespace BookTracker.Api.Endpoints;

public static class BookEndpoints
{
    public static IEndpointRouteBuilder MapBookEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/books", GetBookSummaries);
        app.MapGet("/books/{id:int}", GetBookDetails);
        app.MapPost("/books", CreateBook).RequireAuthorization();
        app.MapPut("/books/{id:int}", UpdateBook).RequireAuthorization();
        app.MapDelete("/books/{id:int}", DeleteBook).RequireAuthorization();

        return app;
    }

    public static async Task<IResult> GetBookSummaries([AsParameters] GetBookSummariesRequest request, GetBookSummariesQueryHandler query)
    => Results.Ok(await query.Execute(request));

    public static async Task<IResult> GetBookDetails(int id, GetBookDetailsQueryHandler query)
    {
        GetBookDetailsResponse? book = await query.Execute(id);
        return book is null ? Results.NotFound() : Results.Ok(book);
    }

    public static async Task<IResult> CreateBook(CreateBookRequest request, ClaimsPrincipal principal, CreateBookCommandHandler handler)
    {
        try
        {
            var actor = principal.ToActor();

            CreateBookResponse response = await handler.Execute(actor, request);
            return Results.Created($"/books/{response.Id}", response);
        }
        catch (ForbiddenOperationException)
        {
            return Results.Forbid();
        }
        catch (DomainException exception)
        {
            return Results.BadRequest(new { error = exception.Message });
        }
    }
    public static async Task<IResult> UpdateBook(int id, UpdateBookRequest request, ClaimsPrincipal principal, UpdateBookCommandHandler handler)
    {
        try
        {
            Actor actor = principal.ToActor();

            UpdateBookResult result = await handler.Execute(actor, id, request);

            return result switch
            {
                UpdateBookResult.Updated => Results.NoContent(),
                UpdateBookResult.NotFound => Results.NotFound(),
                UpdateBookResult.Conflict => Results.Conflict(
                    new { error = "The book was changed by another user." }),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        catch (ForbiddenOperationException)
        {
            return Results.Forbid();
        }
        catch (DomainException exception)
        {
            return Results.BadRequest(new { error = exception.Message });
        }

    }

    public static async Task<IResult> DeleteBook(int id, ClaimsPrincipal principal, DeleteBookCommandHandler handler)
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