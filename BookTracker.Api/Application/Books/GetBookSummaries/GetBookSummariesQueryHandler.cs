using BookTracker.Api.Application.GetBookSummaries;
using BookTracker.Api.Storage;
using Microsoft.EntityFrameworkCore;
using BookTracker.Api.Domain.Books;

namespace BookTracker.Api.Application.Books.GetBookSummaries;

public class GetBookSummariesQueryHandler(AppDbContext dbContext) : IHandler
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 10;
    private const int MinPage = 1;
    private const int MaxPageSize = 50;

    public async Task<PagedResult<BookSummary>> Execute(GetBookSummariesRequest request)
    {
        int page = Math.Max(1, request.Page ?? DefaultPage); // Math.Max(1, 3) >> 3 | Math.Max(1,-8) >> 1
        int pageSize = Math.Clamp(request.PageSize ?? DefaultPageSize, MinPage, MaxPageSize);
        /*
        ?pageSize=20 > 20
        ?pageSize=100 > 50
        ?pageSize=0 > 1
        */
        var query = dbContext.Books.AsNoTracking();


        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            /*
            If the search term contains a NUL character ('\0') ((SQLite(C taal))), SQLite LIKE does not work correctly.
            Daarom we load all books into memory and use .NET's string.Contains() to find the matching books.
            Daarna we collect the IDs and continue the query with these IDs. Zo paging and counting still work correctly...
            And also the SearchByStringTerminatorReturnsExactMatch test passes.
            */

            // Deza allen in SQLite (nooit in PostegreSQL)
            if (request.Search.Contains('\0')) // search contains (\0 >> NULL value) DUS gebruik niet SQL
            {
                string term = request.Search.Trim();
                List<Book> allBooks = await dbContext.Books.AsNoTracking().ToListAsync(); // GET ALL BOOKS
                List<int> matchingIds = allBooks
                    .Where(b => // Deza C# .. Niet SQLite LIKE
                    // Because a String in .NET is not a C String.... In .NET alle String bewaart its actual LENGTH (het stopt niet in \0)
                        b.Title.Value.Contains(term) || b.Author.Value.Contains(term))
                    .Select(b => b.Id)
                    .ToList();
                query = query.Where(b => matchingIds.Contains(b.Id));
                // This means going back to EF Core again... But NOW... not by LIKK... BUT by WHERE Id IN (4,9,11)
            }
            else
            {
                String searchResult = request.Search.Trim().Replace("%", "\\%").Replace("_", "\\_");
                String search = $"%{searchResult}%";

                query = query.Where(book =>
                    EF.Functions.ILike((string)book.Title, search, "\\") ||
                    EF.Functions.ILike((string)book.Author, search, "\\"));
            }
        }
        int totalItems = await query.CountAsync(); // EF Core 

        List<BookSummary> books = await query.AsNoTracking() // Allen lezen
                    .OrderBy(book => book.Id)
                    .Skip((page - 1) * pageSize) // (2 - 1) × 10 = 10
                    .Take(pageSize) // 10 DUS 10 books in page
                    .Select(book =>
                        new BookSummary
                        {
                            Id = book.Id,
                            Title = book.Title.Value,
                            Author = book.Author.Value
                        })
                    .ToListAsync();

        return new GetBookSummariesResponse
        {
            Items = books,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize) // 95 / 10 = 9.5 >> Ceiling(9.5) -> 10
        };

        /*
                {
        "items": [
        {
          "id": 7,
          "title": "Changes",
          "author": "David Bowie"
        },
        {
          "id": 8,
          "title": "YAA",
          "author": "David Bowie"
        },
        {
          "id": 12,
          "title": "Dune",
          "author": "Frank Herbert"
        }
        ],
        "page": 1,
        "pageSize": 10,
        "totalItems": 3,
        "totalPages": 1
        }
        */
    }
}