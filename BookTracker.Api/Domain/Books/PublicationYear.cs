namespace BookTracker.Api.Domain.Books;

public sealed record PublicationYear
{
    public const int MinYear = 1450; // The first book was published on this date 
    public int Value { get; }

    public PublicationYear(int value)
    {
        int maxYear = DateTime.Now.Year + 1;

        if (value < MinYear || value > maxYear)
            throw new DomainException($"Year must be between {MinYear} and {maxYear}.");

        Value = value;
    }

    public static implicit operator int(PublicationYear year) => year.Value;
    public override string ToString() => Value.ToString();
}