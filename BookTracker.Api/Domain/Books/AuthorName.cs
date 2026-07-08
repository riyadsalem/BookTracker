namespace BookTracker.Api.Domain.Books;

public sealed record AuthorName
{
    public const int MaxLength = 100;
    public string Value { get; }
    public AuthorName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Author is required.");

        string cleaned = value.Trim();

        if (cleaned.Length > MaxLength)
            throw new DomainException($"Author cannot be longer than {MaxLength} characters.");
        Value = cleaned;
    }

    public static implicit operator string(AuthorName author) => author.Value;
    public override string ToString() => Value;

}

