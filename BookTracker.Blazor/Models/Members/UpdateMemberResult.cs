namespace BookTracker.Blazor.Models.Members;

public enum UpdateMemberStatus
{
    Updated,
    ValidationFailed,
    Unauthorized,
    Forbidden,
    NotFound,
    EmailConflict
}
public sealed record UpdateMemberResult(UpdateMemberStatus Status, string? ErrorMessage = null);