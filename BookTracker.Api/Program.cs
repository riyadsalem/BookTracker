// using BookTracker.Api.Application;
using BookTracker.Api.Storage;
using Microsoft.EntityFrameworkCore;
using BookTracker.Api.Endpoints;
using BookTracker.Api.Application.BookList;
using BookTracker.Api.Application.GetBookById;
using BookTracker.Api.Application.CreateBook;
using BookTracker.Api.Application.UpdateBook;
using BookTracker.Api.Application.DeleteBook;

var builder = WebApplication.CreateBuilder(args);

// builder.Services.AddSingleton<IBookRepository, InMemoryBookRepository>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("BookTracker"));
});

builder.Services.AddScoped<IBookRepository, EfBookRepository>();

// builder.Services.AddScoped<BookService>(); // When zietne alle services in een file

// SERVICE LAYER ((((I LIKE THAAAAAT))))
builder.Services.AddScoped<GetBookListQuery>();
builder.Services.AddScoped<GetBookByIdQuery>();
builder.Services.AddScoped<CreateBookCommandHandler>();
builder.Services.AddScoped<UpdateBookCommandHandler>();
builder.Services.AddScoped<DeleteBookCommandHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    scope.ServiceProvider
        .GetRequiredService<AppDbContext>()
        .Database
        .EnsureCreated();
}

app.MapBookEndpoints();
app.Run(); // NA ENDPOINT API

public partial class Program;