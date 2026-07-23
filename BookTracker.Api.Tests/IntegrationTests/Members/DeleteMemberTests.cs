using System.Net;
using BookTracker.Api.Domain.Members;

namespace BookTracker.Api.Tests.IntegrationTests.Members;

[Collection(PostgreSqlCollection.Name)]
public class DeleteMemberTests(PostgreSqlFixture database) : IntegrationTest(database)
{
    [Fact]
    public async Task DeleteMemberRemovesMember()
    {
        int memberId = await AuthenticateAsMember();

        var response = await Client.DeleteAsync($"/members/{memberId}");
        await response.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        Member? member = Reader.Query(db => db.Members.Find(1));
        Assert.Null(member);
    }

    [Fact]
    public async Task DeleteMemberReturnsNotFoundWhenMemberDoesNotExist()
    {
        int memberId = await AuthenticateAsMember();

        Writer.Seed(db => db.Members.Remove(db.Members.Find(memberId)!));

        var response = await Client.DeleteAsync($"/members/{memberId}");
        await response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }
}