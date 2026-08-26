namespace BookTracker.Blazor.Models.Members;

public sealed class MemberDetailsResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
}