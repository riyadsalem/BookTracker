using BookTracker.Api.Application;
using BookTracker.Api.Application.CreateBook;
using BookTracker.Api.Application.UpdateBook;
using BookTracker.Api.Storage;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// builder.Services.AddSingleton<IBookRepository, InMemoryBookRepository>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("BookTracker"));
});

builder.Services.AddScoped<IBookRepository, EfBookRepository>();

builder.Services.AddScoped<BookService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    scope.ServiceProvider
        .GetRequiredService<AppDbContext>()
        .Database
        .EnsureCreated();
}

// GET
app.MapGet("/books", async (BookService service) => Results.Ok(await service.GetAllBooks()));

// POST
app.MapPost("/books", async (CreateBookRequest request, BookService service) =>
{
    var response = await service.CreateBook(request);
    return Results.Created($"/books/{response.Id}", response);
});

// DELETE
app.MapDelete("/books/{id:int}", async (int id, BookService service) =>
     await service.DeleteBook(id) ? Results.NoContent() : Results.NotFound());

app.MapPut("/books/{id:int}", async (int id, UpdateBookRequest request, BookService service) =>
await service.UpdateBook(id, request) ? Results.NoContent() : Results.NotFound());

app.MapGet("/books/{id:int}", async (int id, BookService service) =>
{
    var book = await service.GetBookById(id);
    return book is null ? Results.NotFound() : Results.Ok(book);
});

app.Run(); // NA ENDPOINT API

public partial class Program;