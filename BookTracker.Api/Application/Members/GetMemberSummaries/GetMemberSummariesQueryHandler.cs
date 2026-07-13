using BookTracker.Api.Domain.Actors;
using BookTracker.Api.Domain.Members;
using BookTracker.Api.Storage;
using Microsoft.EntityFrameworkCore;

namespace BookTracker.Api.Application.Members.GetMemberSummaries;

public class GetMemberSummariesQueryHandler(AppDbContext dbContext) : IHandler
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 50;

    public async Task<GetMemberSummariesResponse> Execute(Actor actor, GetMemberSummariesRequest request)
    {
        MemberPermissions.EnsureCanViewDirectory(actor);

        int page = Math.Max(DefaultPage, request.Page ?? DefaultPage);
        int pageSize = Math.Clamp(request.PageSize ?? DefaultPageSize, 1, MaxPageSize);

        var query = dbContext.Members.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            String searchResult = request.Search.Trim().Replace("%", "\\%").Replace("_", "\\_");
            String search = $"%{searchResult}%";

            query = query.Where(member =>
                EF.Functions.Like((string)member.Name, search, "\\") ||
                EF.Functions.Like((string)member.Email, search, "\\"));
        }

        int totalItems = await query.CountAsync();

        var members = await query
            .OrderBy(member => member.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(member => new MemberSummary
            {
                Id = member.Id,
                Name = member.Name.Value,
                Email = member.Email.Value
            })
            .ToListAsync();

        return new GetMemberSummariesResponse
        {
            Items = members,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }
}