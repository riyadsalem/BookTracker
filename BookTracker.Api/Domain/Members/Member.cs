namespace BookTracker.Api.Domain.Members;

public class Member
{
    public int Id { get; set; }
    public required MemberName Name { get; set; }
    public required MemberEmail Email { get; set; }

    // No ValueObject for Password - it should be used once and....forgotten, not held around like Name / Email.
    public string PasswordHash { get; set; } = string.Empty;
}