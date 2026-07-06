using BookTracker.Api.Domain.Members;

namespace BookTracker.Api.Storage.Members;

public class EfMemberRepository(AppDbContext dbContext) : IMemberRepository
{
    public async Task<Member> AddAsync(Member member)
    {
        dbContext.Members.Add(member);
        await dbContext.SaveChangesAsync();
        return member;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        Member? member = await dbContext.Members.FindAsync(id);
        if (member is null) return false;

        dbContext.Members.Remove(member);
        await dbContext.SaveChangesAsync();
        return true;
    }



    public async Task<bool> UpdateAsync(Member member)
    {
        Member? existing = await dbContext.Members.FindAsync(member.Id);

        if (existing is null) return false;

        existing.Name = member.Name;
        existing.Email = member.Email;

        await dbContext.SaveChangesAsync();

        return true;
    }
}

