using BookTracker.Api.Storage.Members;
namespace BookTracker.Api.Application.Members.DeleteMember;

public class DeleteMemberCommandHandler(IMemberRepository repository) : IHandler
{
    public async Task<bool> Execute(int id) => await repository.DeleteAsync(id);

}
