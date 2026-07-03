using BookTracker.Api.Application.BookList;
using BookTracker.Api.Application.CreateBook;
using BookTracker.Api.Application.GetBookById;
using BookTracker.Api.Application.UpdateBook;
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
            Title = book.Title.Value, // ValueObject van DB DUS book.Title.Value....
            Author = book.Author.Value
        }).ToList();
    }

    public async Task<CreateBookResponse> CreateBook(CreateBookRequest request)
    {
        Book? book = new Book
        {
            Title = new BookTitle(request.Title), // ValueObject vooooor DB DUS new BookTitle()....
            Author = new AuthorName(request.Author),
            Year = request.Year
        };
        await bookRepository.AddAsync(book);
        return new CreateBookResponse
        {
            Id = book.Id,
            Title = book.Title.Value,
            Author = book.Author.Value,
            Year = book.Year
        }
        ;
    }

    public async Task<bool> DeleteBook(int id) =>
         await bookRepository.DeleteAsync(id);

    public async Task<bool> UpdateBook(int id, UpdateBookRequest request) =>
         await bookRepository.UpdateAsync(new Book
         {
             Id = id,
             Title = new BookTitle(request.Title),
             Author = new AuthorName(request.Author),
             Year = request.Year
         });

    public async Task<BookDetails?> GetBookById(int id)
    {
        Book? book = await bookRepository.GetByIdAsync(id);

        if (book is null) return null;

        return new BookDetails
        {
            Id = book.Id,
            Title = book.Title.Value,
            Author = book.Author.Value,
            Year = book.Year
        };
    }
}
