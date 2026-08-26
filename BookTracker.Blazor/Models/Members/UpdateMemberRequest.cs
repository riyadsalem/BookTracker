namespace BookTracker.Blazor.Models.Members;

public sealed class UpdateMemberRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}