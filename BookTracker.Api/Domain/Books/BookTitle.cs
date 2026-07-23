namespace BookTracker.Api.Domain.Books;

public sealed record BookTitle
{
    public const int MaxLength = 100;
    public string Value { get; }
    public BookTitle(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Title is required.");

        if (value.Contains('\0'))
            throw new DomainException("Title cannot contain a null character.");

        string cleaned = value.Trim();

        if (cleaned.Length > MaxLength)
            throw new DomainException($"Title cannot be longer than {MaxLength} characters.");
        Value = cleaned;
    }
    /*
    When I request a String from a BookTitle, it gives me a Value.
    book.Title.Value >> (string)book.Title
    */
    public static implicit operator string(BookTitle title) => title.Value;
    public override string ToString() => Value;
}