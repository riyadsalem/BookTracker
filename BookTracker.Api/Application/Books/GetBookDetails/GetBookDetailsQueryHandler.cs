using BookTracker.Api.Storage;
using Microsoft.EntityFrameworkCore;

namespace BookTracker.Api.Application.Books.GetBookDetails;

public class GetBookDetailsQueryHandler(AppDbContext dbContext) : IHandler
{
    public async Task<GetBookDetailsResponse?> Execute(int id)
    {
        return await dbContext.Books
            .AsNoTracking() // No tracking = faster read-only queries.
            .Where(book => book.Id == id)
            .Select(book =>
                new GetBookDetailsResponse
                {
                    Id = book.Id,
                    Title = book.Title.Value,
                    Author = book.Author.Value,
                    Year = book.Year,
                    Version = book.Version
                })
            .FirstOrDefaultAsync();
    }
}