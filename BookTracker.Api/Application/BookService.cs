using BookTracker.Api.Application.BookList;
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
}
