using BookTracker.Api.Domain.Members;
using BookTracker.Api.Storage.Members;

namespace BookTracker.Api.Application.Members.CreateMember;

public class CreateMemberCommandHandler(IMemberRepository repository) : IHandler
{
    public async Task<CreateMemberResponse> Execute(CreateMemberRequest request)
    {
        Member member = new()
        {
            Name = new MemberName(request.Name),
            Email = new MemberEmail(request.Email)
        };

        Member savedMember = await repository.AddAsync(member);

        return new CreateMemberResponse
        {
            Id = savedMember.Id,
            Name = savedMember.Name.Value,
            Email = savedMember.Email.Value
        };
    }
}