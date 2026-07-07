using System.Net;
using System.Net.Http.Json;
using BookTracker.Api.Application.Members.UpdateMember;
using BookTracker.Api.Domain.Members;

namespace BookTracker.Api.Tests.IntegrationTests.Members;

public class UpdateMemberTests : IntegrationTest
{
    [Fact]
    public async Task PutMemberUpdatesMember()
    {
        Writer.Seed(db =>
        {
            db.Members.Add(
                new Member
                {
                    Name = new MemberName("Riyad"),
                    Email = new MemberEmail("r@gmail.com")
                });
        });

        UpdateMemberRequest request = new()
        {
            Name = "Mark",
            Email = "mark@gmail.com"
        };

        var response = await Client.PutAsJsonAsync("/members/1", request);

        await response.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        Member? member = Reader.Query(db => db.Members.Find(1));

        Assert.NotNull(member);
        Assert.Equal("Mark", member.Name.Value);
        Assert.Equal("mark@gmail.com", member.Email.Value);
    }

    [Fact]
    public async Task PutMemberReturnsNotFoundWhenMemberDoesNotExist()
    {
        UpdateMemberRequest request = new()
        {
            Name = "Riyad",
            Email = "riyad@gmail.com"
        };

        var response = await Client.PutAsJsonAsync("/members/9999", request);
        await response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PutMemberReturnsBadRequestWhenNameIsWhitespace()
    {
        Writer.Seed(db =>
        {
            db.Members.Add(
                new Member
                {
                    Name = new MemberName("Riyad"),
                    Email = new MemberEmail("r@gmail.com")
                });
        });

        UpdateMemberRequest request = new()
        {
            Name = "   ",
            Email = "r@gmail.com"
        };

        var response = await Client.PutAsJsonAsync("/members/1", request);
        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutMemberReturnsBadRequestWhenEmailIsWhitespace()
    {
        Writer.Seed(db =>
        {
            db.Members.Add(
                new Member
                {
                    Name = new MemberName("Riyad"),
                    Email = new MemberEmail("r@gmail.com")
                });
        });

        UpdateMemberRequest request = new()
        {
            Name = "Riyad",
            Email = "   "
        };

        var response = await Client.PutAsJsonAsync("/members/1", request);
        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutMemberReturnsBadRequestWhenEmailHasNoAtSign()
    {
        Writer.Seed(db =>
        {
            db.Members.Add(
                new Member
                {
                    Name = new MemberName("riyad"),
                    Email = new MemberEmail("r@gmail.com")
                });
        });

        UpdateMemberRequest request = new()
        {
            Name = "Riyad",
            Email = "rgmail.com"
        };

        var response = await Client.PutAsJsonAsync("/members/1", request);
        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

}