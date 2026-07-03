namespace BookTracker.Api.Application;

public class PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; set; } // zoals books
    public int Page { get; set; } // page number
    public int PageSize { get; set; } // items in page 
    public int TotalItems { get; set; } // books in DB
    public int TotalPages { get; set; } // total pages heb ik 
}