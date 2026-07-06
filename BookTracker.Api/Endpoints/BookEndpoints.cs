using BookTracker.Api.Application.Books.CreateBook;
using BookTracker.Api.Application.Books.DeleteBook;
using BookTracker.Api.Application.Books.GetBookDetails;
using BookTracker.Api.Application.Books.GetBookSummaries;
using BookTracker.Api.Application.Books.UpdateBook;
using BookTracker.Api.Domain;

namespace BookTracker.Api.Endpoints;

public static class BookEndpoints
{
    public static IEndpointRouteBuilder MapBookEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/books", GetBookSummaries);
        app.MapGet("/books/{id:int}", GetBookDetails);
        app.MapPost("/books", CreateBook);
        app.MapPut("/books/{id:int}", UpdateBook);
        app.MapDelete("/books/{id:int}", DeleteBook);
        return app;
    }

    /*
    request.page & request.pageSize
    GET /book?page=1&pageSize=10 >>>> [AsParameters] > ASP.NET lees page,pagesize van LINK
    */
    public static async Task<IResult> GetBookSummaries([AsParameters] GetBookSummariesRequest request, GetBookSummariesQueryHandler query)
    => Results.Ok(await query.Execute(request));

    public static async Task<IResult> GetBookDetails(int id, GetBookDetailsQueryHandler query)
    {
        GetBookDetailsResponse? book = await query.Execute(id);
        return book is null ? Results.NotFound() : Results.Ok(book);
    }

    public static async Task<IResult> CreateBook(CreateBookRequest request, CreateBookCommandHandler handler)
    {
        try
        {
            CreateBookResponse response = await handler.Execute(request);
            return Results.Created($"/books/{response.Id}", response);
        }
        catch (DomainException exception)
        {
            return Results.BadRequest(new { error = exception.Message });
        }
    }
    public static async Task<IResult> UpdateBook(int id, UpdateBookRequest request, UpdateBookCommandHandler handler)
    {
        try
        {
            return await handler.Execute(id, request) ? Results.NoContent() : Results.NotFound();
            // Results.NotFound() (Errors hier van req(ID IS NOT FOUND)) >> 404
        }
        catch (DomainException exception)
        {
            return Results.BadRequest(new { error = exception.Message });
            // Errors hier van (ObjectValues en ....) 400
        }

    }

    public static async Task<IResult> DeleteBook(int id, DeleteBookCommandHandler handler) =>
    await handler.Execute(id) ? Results.NoContent() : Results.NotFound();

}