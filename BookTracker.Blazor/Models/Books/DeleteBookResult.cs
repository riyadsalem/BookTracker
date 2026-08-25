namespace BookTracker.Blazor.Models.Books;

public enum DeleteBookStatus
{
    Deleted,
    Unauthorized,
    Forbidden,
    NotFound
}
public sealed record DeleteBookResult(DeleteBookStatus Status);