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

/*
// zonder paging
// zonder repo layer 
// Query > AppDbContext > DTOs (((SNELLER)))
// Direct projection to DTOs (no Repository, no Entity loading).
public class GetBookListQuery(AppDbContext dbContext)
{
    public async Task<IReadOnlyList<BookInfo>> Execute(GetBookListRequest request) =>
    await dbContext.Books.AsNoTracking().Select(book => new BookInfo
    {
        Id = book.Id,
        Title = book.Title.Value,
        Author = book.Author.Value
    }).ToListAsync();

}
*/

public class GetBookListQuery(AppDbContext dbContext)
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 10;
    private const int MinPage = 1;
    private const int MaxPageSize = 50;

    public async Task<PagedResult<BookInfo>> Execute(GetBookListRequest request)
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
            // SQL (LIKE '%dune%') >>> It is looking for any text that contains the word dune.
            var search = $"%{request.Search.Trim()}%";

            query = query.Where(book =>
            // (string)book.Title (DDD) >>>> book.Title.Value
                EF.Functions.Like((string)book.Title, search) || // Like("Dune Messiah", "%dune%")
                EF.Functions.Like((string)book.Author, search));
        }

        int totalItems = await query.CountAsync(); // EF Core 

        List<BookInfo> books = await query.AsNoTracking() // Allen lezen
                    .OrderBy(book => book.Id)
                    .Skip((page - 1) * pageSize) // (2 - 1) × 10 = 10
                    .Take(pageSize) // 10 DUS 10 books in page
                    .Select(book =>
                        new BookInfo
                        {
                            Id = book.Id,
                            Title = book.Title.Value,
                            Author = book.Author.Value
                        })
                    .ToListAsync();

        return
            new PagedResult<BookInfo>
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