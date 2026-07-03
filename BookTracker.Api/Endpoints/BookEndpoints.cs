using BookTracker.Api.Application.BookList;
using BookTracker.Api.Application.CreateBook;
using BookTracker.Api.Application.DeleteBook;
using BookTracker.Api.Application.GetBookById;
using BookTracker.Api.Application.UpdateBook;
using BookTracker.Api.Domain;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookTracker.Api.Endpoints;

public static class BookEndpoints
{
    public static IEndpointRouteBuilder MapBookEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/books", GetAllBooks);
        app.MapGet("/books/{id:int}", GetBookById);
        app.MapPost("/books", CreateBook);
        app.MapPut("/books/{id:int}", UpdateBook);
        app.MapDelete("/books/{id:int}", DeleteBook);
        return app;
    }

    /*
    request.page & request.pageSize
    GET /book?page=1&pageSize=10 >>>> [AsParameters] > ASP.NET lees page,pagesize van LINK
    */
    public static async Task<IResult> GetAllBooks([AsParameters] GetBookListRequest request, GetBookListQuery query)
    => Results.Ok(await query.Execute(request));

    public static async Task<IResult> GetBookById(int id, GetBookByIdQuery query)
    {
        BookDetails? book = await query.Execute(id);
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