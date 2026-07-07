using System.Net;
using BookTracker.Api.Domain.Members;

namespace BookTracker.Api.Tests.IntegrationTests.Members;

public class DeleteMemberTests : IntegrationTest
{
    [Fact]
    public async Task DeleteMemberRemovesMember()
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

        var response = await Client.DeleteAsync("/members/1");
        await response.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        Member? member = Reader.Query(db => db.Members.Find(1));
        Assert.Null(member);
    }

    [Fact]
    public async Task DeleteMemberReturnsNotFoundWhenMemberDoesNotExist()
    {
        var response = await Client.DeleteAsync("/members/9999");
        await response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }
}