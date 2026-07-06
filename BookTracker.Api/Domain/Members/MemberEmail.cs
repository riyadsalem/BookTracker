namespace BookTracker.Api.Domain.Members;

public record MemberEmail
{
    public const int MaxLength = 200;
    public string Value { get; }
    public MemberEmail(string value)
    {
        value = value.Trim();

        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email is required.");

        if (value.Length > MaxLength)
            throw new DomainException($"Email cannot exceed {MaxLength} characters.");

        if (!value.Contains('@'))
            throw new DomainException("Email must contain '@'.");

        Value = value;
    }

    public static implicit operator string(MemberEmail email) => email.Value;

}