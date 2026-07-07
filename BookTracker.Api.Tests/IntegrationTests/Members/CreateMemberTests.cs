using System.Net;
using System.Net.Http.Json;
using BookTracker.Api.Application.Members.CreateMember;
using BookTracker.Api.Domain.Members;

namespace BookTracker.Api.Tests.IntegrationTests.Members;

public class CreateMemberTests : IntegrationTest
{

    [Fact]
    public async Task PostMemberCreatesMember()
    {
        CreateMemberRequest request = new()
        {
            Name = "Riyad",
            Email = "r@gmail.com"
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
    }

    [Fact]
    public async Task PostMemberReturnsBadRequestWhenNameIsWhitespace()
    {
        CreateMemberRequest request = new()
        {
            Name = "   ",
            Email = "r@gmail.com"
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
            Email = "   "
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
            Email = "r.gmail.com"
        };

        var response = await Client.PostAsJsonAsync("/members", request);
        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

}