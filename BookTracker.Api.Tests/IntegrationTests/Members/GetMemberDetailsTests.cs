using System.Net;
using BookTracker.Api.Application.Members.GetMemberDetails;
using BookTracker.Api.Domain.Members;

namespace BookTracker.Api.Tests.IntegrationTests.Members;

public class GetMemberDetailsTests : IntegrationTest
{
    [Fact]
    public async Task GetMemberDetailsReturnsMember()
    {
        Writer.Seed(db =>
        {
            db.Members.Add(
                new Member
                {
                    Name = new MemberName("Riyad"),
                    Email = new MemberEmail("r@gmail.com"),
                    PasswordHash = "123456789"
                });
        });

        var response = await Client.GetAsync("/members/1");
        GetMemberDetailsResponse member = await response.ReadJsonAs<GetMemberDetailsResponse>(HttpStatusCode.OK);

        Assert.NotNull(member);
        Assert.Equal(1, member.Id);
        Assert.Equal("Riyad", member.Name);
        Assert.Equal("r@gmail.com", member.Email);
    }

    [Fact]
    public async Task GetMemberDetailsReturnsNotFoundWhenMemberDoesNotExist()
    {
        var response = await Client.GetAsync("/members/9999");
        await response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }
}
