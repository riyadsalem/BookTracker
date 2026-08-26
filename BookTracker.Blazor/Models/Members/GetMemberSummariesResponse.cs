namespace BookTracker.Blazor.Models.Members;

public sealed class GetMemberSummariesResponse
{
    public required IReadOnlyList<MemberSummary> Items { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}

