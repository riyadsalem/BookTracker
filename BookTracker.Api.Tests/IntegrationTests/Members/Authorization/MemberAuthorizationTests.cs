using System.Net;
using System.Net.Http.Json;
using BookTracker.Api.Application.Members.CreateMember;
using BookTracker.Api.Application.Members.UpdateMember;
using BookTracker.Api.Domain.Members;

namespace BookTracker.Api.Tests.IntegrationTests.Members.Authorization;

[Collection(PostgreSqlCollection.Name)]
public class MemberAuthorizationTests(PostgreSqlFixture database) : IntegrationTest(database)
{
    private int SeedMember(
        string name = "Grace Hopper",
        string email = "grace@example.com")
    {
        var member = new Member
        {
            Name = new MemberName(name),
            Email = new MemberEmail(email),
            PasswordHash = "test-password-hash"
        };

        Writer.Seed(db => db.Members.Add(member));

        return member.Id;
    }

    [Fact]
    public async Task CreateMemberDoesNotRequireAuthentication()
    {
        CreateMemberRequest request = new()
        {
            Name = "Grace Hopper",
            Email = "grace@example.com",
            Password = "debugging-moth"
        };

        var response = await Client.PostAsJsonAsync("/members", request);

        await response.ShouldHaveStatusCode(HttpStatusCode.Created);
    }

    [Fact]
    public async Task UpdateMemberRequiresAuthentication()
    {
        int memberId = SeedMember();

        UpdateMemberRequest request = new()
        {
            Name = "Ada Byron",
            Email = "ada.byron@example.com"
        };

        var response = await Client.PutAsJsonAsync($"/members/{memberId}", request);

        await response.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteMemberRequiresAuthentication()
    {
        int memberId = SeedMember();

        var response = await Client.DeleteAsync($"/members/{memberId}");

        await response.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);

        Member? member = Reader.Query(db => db.Members.Find(memberId));
        Assert.NotNull(member);
    }

    [Fact]
    public async Task MemberCanUpdateOwnAccount()
    {
        int memberId = await AuthenticateAsMember();

        var request = new UpdateMemberRequest
        {
            Name = "Ada Byron",
            Email = "ada.byron@example.com"
        };

        var response = await Client.PutAsJsonAsync($"/members/{memberId}", request);

        await response.ShouldHaveStatusCode(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task MemberCannotUpdateAnotherMember()
    {
        await AuthenticateAsMember();

        int otherMemberId = SeedMember("Grace Hopper", "grace@example.com");

        UpdateMemberRequest request = new()
        {
            Name = "Changed Name",
            Email = "changed@example.com"
        };

        var response = await Client.PutAsJsonAsync($"/members/{otherMemberId}", request);

        await response.ShouldHaveStatusCode(HttpStatusCode.Forbidden);

        Member? member = Reader.Query(db => db.Members.Find(otherMemberId));

        Assert.NotNull(member);
        Assert.Equal("Grace Hopper", member.Name.Value);
        Assert.Equal("grace@example.com", member.Email.Value);
    }

    [Fact]
    public async Task MemberCannotDeleteAnotherMember()
    {
        await AuthenticateAsMember();

        int otherMemberId = SeedMember("Grace Hopper", "grace@example.com");

        var response = await Client.DeleteAsync($"/members/{otherMemberId}");

        await response.ShouldHaveStatusCode(HttpStatusCode.Forbidden);

        Member? member = Reader.Query(db => db.Members.Find(otherMemberId));
        Assert.NotNull(member);
    }

    [Fact]
    public async Task MemberListRequiresAuthentication()
    {
        var response = await Client.GetAsync("/members");
        await response.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RegularMemberCannotViewMemberList()
    {
        await AuthenticateAsMember();
        var response = await Client.GetAsync("/members");
        await response.ShouldHaveStatusCode(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdministratorCanViewMemberList()
    {
        await AuthenticateAsMember(MemberRole.Administrator);
        var response = await Client.GetAsync("/members");
        await response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RegularMemberCannotViewMemberDetails()
    {
        await AuthenticateAsMember();
        int otherMemberId = SeedMember("Grace Hopper", "grace@example.com");
        var response = await Client.GetAsync($"/members/{otherMemberId}");
        await response.ShouldHaveStatusCode(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdministratorCanViewMemberDetails()
    {
        await AuthenticateAsMember(MemberRole.Administrator);
        int otherMemberId = SeedMember("Grace Hopper", "grace@example.com");
        var response = await Client.GetAsync($"/members/{otherMemberId}");
        await response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AdministratorCanUpdateAnotherMember()
    {
        int otherMemberId = SeedMember("III", "III@gmail.com");

        await AuthenticateAsMember(MemberRole.Administrator);

        UpdateMemberRequest request = new()
        {
            Name = "newIII",
            Email = "newIII@gmail.com"
        };

        var response = await Client.PutAsJsonAsync($"/members/{otherMemberId}", request);

        await response.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        Member? member = Reader.Query(db => db.Members.Find(otherMemberId));
        Assert.NotNull(member);
        Assert.Equal("newIII", member.Name.Value);
    }

    [Fact]
    public async Task AdministratorCanDeleteAnotherMember()
    {
        int otherMemberId = SeedMember("III", "III@gmail.com");

        await AuthenticateAsMember(MemberRole.Administrator);

        var response = await Client.DeleteAsync($"/members/{otherMemberId}");

        await response.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        Member? member = Reader.Query(db => db.Members.Find(otherMemberId));
        Assert.Null(member);
    }
}