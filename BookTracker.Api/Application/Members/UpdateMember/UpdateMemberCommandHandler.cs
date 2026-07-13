using BookTracker.Api.Domain.Actors;
using BookTracker.Api.Domain.Members;
using BookTracker.Api.Storage.Members;

namespace BookTracker.Api.Application.Members.UpdateMember;

public class UpdateMemberCommandHandler(IMemberRepository repository) : IHandler
{
    public async Task<bool> Execute(Actor actor, int id, UpdateMemberRequest request)
    {
        MemberPermissions.EnsureCanManage(actor, id);

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