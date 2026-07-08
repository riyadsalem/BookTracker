namespace BookTracker.Api.Domain.Members;

public record MemberName
{
    public const int MaxLength = 100;
    public string Value { get; }
    public MemberName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Member name is required.");

        value = value.Trim();

        if (value.Length > MaxLength)
            throw new DomainException($"Member name cannot exceed {MaxLength} characters.");

        Value = value;
    }

    public static implicit operator string(MemberName name) => name.Value;

}