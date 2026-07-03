using BookTracker.Api.Application;
using BookTracker.Api.Application.BookList;
using BookTracker.Api.Application.CreateBook;
using BookTracker.Api.Application.GetBookById;
using BookTracker.Api.Application.UpdateBook;
using BookTracker.Api.Domain;

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

    public static async Task<IResult> GetAllBooks(GetBookListQuery query)
    {
        var books = await query.Execute();
        return Results.Ok(books);
    }
    public static async Task<IResult> GetBookById(int id, GetBookByIdQuery query)
    {
        var book = await query.Execute(id);
        return book is null ? Results.NotFound() : Results.Ok(book);
    }

    public static async Task<IResult> CreateBook(CreateBookRequest request, BookService service)
    {
        try
        {
            CreateBookResponse response = await service.CreateBook(request);
            return Results.Created($"/books/{response.Id}", response);
        }
        catch (DomainException exception)
        {
            return Results.BadRequest(new { error = exception.Message });
        }
    }
    public static async Task<IResult> UpdateBook(int id, UpdateBookRequest request, BookService service)
    {
        try
        {
            return await service.UpdateBook(id, request) ? Results.NoContent() : Results.NotFound();
            // Results.NotFound() (Errors hier van req(ID IS NOT FOUND)) >> 404
        }
        catch (DomainException exception)
        {
            return Results.BadRequest(new { error = exception.Message });
            // Errors hier van (ObjectValues en ....) 400
        }

    }

    public static async Task<IResult> DeleteBook(int id, BookService service) =>
    await service.DeleteBook(id) ? Results.NoContent() : Results.NotFound();

}