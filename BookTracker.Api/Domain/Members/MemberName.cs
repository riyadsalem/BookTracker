namespace BookTracker.Api.Domain.Members;

public record MemberName
{
    public const int MaxLength = 100;
    public string Value { get; }
    public MemberName(string value)
    {
        value = value.Trim();

        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Member name is required.");

        if (value.Length > MaxLength)
            throw new DomainException($"Member name cannot exceed {MaxLength} characters.");

        Value = value;
    }

    public static implicit operator string(MemberName name) => name.Value;

}