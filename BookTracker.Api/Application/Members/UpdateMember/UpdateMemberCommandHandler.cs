using BookTracker.Api.Domain.Members;
using BookTracker.Api.Storage.Members;

namespace BookTracker.Api.Application.Members.UpdateMember;

public class UpdateMemberCommandHandler(IMemberRepository repository) : IHandler
{
    public async Task<bool> Execute(int id, UpdateMemberRequest request)
    {
        var name = new MemberName(request.Name);
        var email = new MemberEmail(request.Email);

        if (await repository.EmailExistsAsync(email, id))
            throw new MemberEmailAlreadyExistsException();

        return await repository.UpdateAsync(new Member
        {
            Id = id,
            Name = name,
            Email = email
        });
    }
}