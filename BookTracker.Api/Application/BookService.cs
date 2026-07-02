using BookTracker.Api.Application.BookList;
using BookTracker.Api.Application.CreateBook;
using BookTracker.Api.Domain;
using BookTracker.Api.Storage;

namespace BookTracker.Api.Application;

public class BookService(IBookRepository bookRepository)
{
    public async Task<IReadOnlyList<BookInfo>> GetAllBooks()
    {
        var books = await bookRepository.GetAllAsync();
        return books.Select(book => new BookInfo
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author
        }).ToList();
    }

    public async Task<CreateBookResponse> CreateBook(CreateBookRequest request)
    {
        var book =
            new Book
            {
                Title = request.Title,
                Author = request.Author,
                Year = request.Year
            };
        await bookRepository.AddAsync(book);
        return
            new CreateBookResponse
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Year = book.Year
            };
    }
}
