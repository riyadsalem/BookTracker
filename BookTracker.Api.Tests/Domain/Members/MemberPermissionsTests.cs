using BookTracker.Api.Domain;
using BookTracker.Api.Domain.Actors;
using BookTracker.Api.Domain.Members;

namespace BookTracker.Api.Tests.Domain.Members;

public class MemberPermissionsTests
{
    [Fact]
    public void AdministratorCanViewDirectory()
    {
        Actor actor = new(1, MemberRole.Administrator);
        MemberPermissions.EnsureCanViewDirectory(actor);
    }

    [Fact]
    public void RegularMemberCannotViewDirectory()
    {
        Actor actor = new(1, MemberRole.Member);
        Assert.Throws<ForbiddenOperationException>(() => MemberPermissions.EnsureCanViewDirectory(actor));
    }

    [Fact]
    public void MemberCanManageOwnAccount()
    {
        Actor actor = new(42, MemberRole.Member);
        MemberPermissions.EnsureCanManage(actor, 42);
    }

    [Fact]
    public void MemberCannotManageAnotherAccount()
    {
        Actor actor = new(42, MemberRole.Member);
        Assert.Throws<ForbiddenOperationException>(() => MemberPermissions.EnsureCanManage(actor, 99));
    }

    [Fact]
    public void AdministratorCanManageAnotherAccount()
    {
        Actor actor = new(1, MemberRole.Administrator);
        MemberPermissions.EnsureCanManage(actor, 99);
    }
}