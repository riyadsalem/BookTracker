namespace BookTracker.Api.Domain;

public sealed record BookTitle
{
    public const int MaxLength = 100;
    public string Value { get; }
    public BookTitle(string value)
    {
        string cleaned = value.Trim();

        if (string.IsNullOrWhiteSpace(cleaned))
            throw new DomainException("Title is required.");

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