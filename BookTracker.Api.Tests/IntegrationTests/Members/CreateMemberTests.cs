using System.IO.MemoryMappedFiles;
using System.Net;
using System.Net.Http.Json;
using BookTracker.Api.Application.Members.CreateMember;
using BookTracker.Api.Domain.Members;
using Microsoft.AspNetCore.Identity;

namespace BookTracker.Api.Tests.IntegrationTests.Members;

public class CreateMemberTests : IntegrationTest
{

    [Fact]
    public async Task PostMemberCreatesMember()
    {
        CreateMemberRequest request = new()
        {
            Name = "Riyad",
            Email = "r@gmail.com",
            Password = "analytical-engine"
        };

        var response = await Client.PostAsJsonAsync("/members", request);
        CreateMemberResponse? created = await response.ReadJsonAs<CreateMemberResponse>(HttpStatusCode.Created);

        Assert.NotNull(created);
        Assert.True(created.Id > 0);
        Assert.Equal("Riyad", created.Name);
        Assert.Equal("r@gmail.com", created.Email);

        Member? member = Reader.Query(context => context.Find<Member>(created.Id));

        Assert.NotNull(member);
        Assert.Equal("Riyad", member.Name.Value);
        Assert.Equal("r@gmail.com", member.Email.Value);
        Assert.NotEqual("analytical-engine", member.PasswordHash);

        PasswordHasher<Member> passwordHasher = new();
        PasswordVerificationResult result = passwordHasher.VerifyHashedPassword(member, member.PasswordHash, "analytical-engine");
        Assert.Equal(PasswordVerificationResult.Success, result);

    }

    [Fact]
    public async Task PostMemberReturnsBadRequestWhenNameIsWhitespace()
    {
        CreateMemberRequest request = new()
        {
            Name = "   ",
            Email = "r@gmail.com",
            Password = "analytical-engine"

        };

        var response = await Client.PostAsJsonAsync("/members", request);
        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostMemberReturnsBadRequestWhenEmailIsWhitespace()
    {
        CreateMemberRequest request = new()
        {
            Name = "Riyad",
            Email = "   ",
            Password = "analytical-engine"

        };

        var response = await Client.PostAsJsonAsync("/members", request);
        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostMemberReturnsBadRequestWhenEmailHasNoAtSign()
    {
        CreateMemberRequest request = new()
        {
            Name = "Riyad",
            Email = "r.gmail.com",
            Password = "analytical-engine"

        };

        var response = await Client.PostAsJsonAsync("/members", request);
        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostMemberReturnsBadRequestWhenPasswordIsEmpty()
    {
        CreateMemberRequest request = new()
        {
            Name = "Riyad",
            Email = "r.gmail.com",
            Password = ""

        };

        var response = await Client.PostAsJsonAsync("/members", request);
        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostMemberReturnsBadRequestWhenPasswordIsTooShort()
    {
        CreateMemberRequest request = new()
        {
            Name = "Riyad",
            Email = "r.gmail.com",
            Password = "1234567"

        };

        var response = await Client.PostAsJsonAsync("/members", request);
        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostMemberReturnsConflictWhenEmailAlreadyExists()
    {
        CreateMemberRequest firstRequest = new()
        {
            Name = "Riyad",
            Email = "r@gmail.com",
            Password = "123456789"

        };
        await Client.PostAsJsonAsync("/members", firstRequest);

        CreateMemberRequest secondRequest = new()
        {
            Name = "Riyad",
            Email = "r@gmail.com",
            Password = "123456789"

        };
        var response = await Client.PostAsJsonAsync("/members", secondRequest);
        await response.ShouldHaveStatusCode(HttpStatusCode.Conflict);

    }

    [Fact]
    public async Task CreateMemberCreatesRegularMember()
    {

        CreateMemberRequest request = new()
        {
            Name = "Grace Hopper",
            Email = "grace@example.com",
            Password = "debugging-moth"
        };

        var response = await Client.PostAsJsonAsync("/members", request);
        CreateMemberResponse created = await response.ReadJsonAs<CreateMemberResponse>(HttpStatusCode.Created);

        Member? member = Reader.Query(db => db.Members.Find(created.Id));

        Assert.NotNull(member);
        Assert.Equal(MemberRole.Member, member.Role);
    }

}