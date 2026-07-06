using BookTracker.Api.Domain;
using BookTracker.Api.Storage;

namespace BookTracker.Api.Application.CreateBook;

public class CreateBookCommandHandler(IBookRepository bookRepository) : IHandler
{
    public async Task<CreateBookResponse> Execute(CreateBookRequest request)
    {
        Book book = new()
        {
            Title = new BookTitle(request.Title),
            Author = new AuthorName(request.Author),
            Year = request.Year
        };

        Book savedBook = await bookRepository.AddAsync(book);

        return new CreateBookResponse
        {
            Id = savedBook.Id,
            Title = savedBook.Title.Value,
            Author = savedBook.Author.Value,
            Year = savedBook.Year
        };
    }
}