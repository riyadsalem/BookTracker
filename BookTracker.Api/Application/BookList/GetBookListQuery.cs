using BookTracker.Api.Storage;
using Microsoft.EntityFrameworkCore;

namespace BookTracker.Api.Application.BookList;

/* // hier gebruijk ik repo layer
// Query > IRepo > Book(Entity) > DTOs
public class GetBookListQuery(IBookRepository bookRepository)
{
    public async Task<IReadOnlyList<BookInfo>> Execute()
    {
        var books = await bookRepository.GetAllAsync();
        return books.Select(book => new BookInfo
        {
            Id = book.Id,
            Title = book.Title.Value, // ValueObject van DB DUS book.Title.Value....
            Author = book.Author.Value
        }).ToList();
    }
}
*/

// zonder repo layer 
// Query > AppDbContext > DTOs (((SNELLER)))
// Direct projection to DTOs (no Repository, no Entity loading).
public class GetBookListQuery(AppDbContext dbContext)
{
    public async Task<IReadOnlyList<BookInfo>> Execute() =>
    await dbContext.Books.AsNoTracking().Select(book => new BookInfo
    {
        Id = book.Id,
        Title = book.Title.Value,
        Author = book.Author.Value
    }).ToListAsync();

}