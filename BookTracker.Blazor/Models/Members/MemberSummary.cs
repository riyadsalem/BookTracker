namespace BookTracker.Blazor.Models.Members;

public sealed class MemberSummary
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
}