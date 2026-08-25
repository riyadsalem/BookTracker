namespace BookTracker.Blazor.Models.Books;

public enum UpdateBookStatus
{
    Updated, // 204
    ValidationFailed, // 400 (Bad Request)
    Unauthorized, // 401
    Forbidden, // 403
    NotFound, // 404
    Conflict // 409
}

public sealed record UpdateBookResult(UpdateBookStatus Status, string? ErrorMessage = null);