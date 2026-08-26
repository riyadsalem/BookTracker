namespace BookTracker.Blazor.Models.Members;

public enum DeleteMemberStatus
{
    Deleted,
    Unauthorized,
    Forbidden,
    NotFound
}
public sealed record DeleteMemberResult(DeleteMemberStatus Status);