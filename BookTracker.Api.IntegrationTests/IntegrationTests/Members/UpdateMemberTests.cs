using System.Net;
using System.Net.Http.Json;
using BookTracker.Api.Application.Members.UpdateMember;
using BookTracker.Api.Domain.Members;

namespace BookTracker.Api.IntegrationTests.Members;

[Collection(PostgreSqlCollection.Name)]
public class UpdateMemberTests(PostgreSqlFixture database) : IntegrationTest(database)
{
    [Fact]
    public async Task PutMemberUpdatesMember()
    {
        int memberId = await AuthenticateAsMember();

        UpdateMemberRequest request = new()
        {
            Name = "Mark",
            Email = "mark@gmail.com"
        };

        var response = await Client.PutAsJsonAsync($"/members/{memberId}", request);

        await response.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        Member? member = Reader.Query(db => db.Members.Find(memberId));

        Assert.NotNull(member);
        Assert.Equal("Mark", member.Name.Value);
        Assert.Equal("mark@gmail.com", member.Email.Value);
    }

    [Fact]
    public async Task PutMemberReturnsNotFoundWhenMemberDoesNotExist()
    {
        int memberId = await AuthenticateAsMember();

        Writer.Seed(db => db.Members.Remove(db.Members.Find(memberId)!));


        UpdateMemberRequest request = new()
        {
            Name = "Riyad",
            Email = "riyad@gmail.com"
        };

        var response = await Client.PutAsJsonAsync($"/members/{memberId}", request);
        await response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PutMemberReturnsBadRequestWhenNameIsWhitespace()
    {
        int memberId = await AuthenticateAsMember();

        UpdateMemberRequest request = new()
        {
            Name = "   ",
            Email = "r@gmail.com"
        };

        var response = await Client.PutAsJsonAsync($"/members/{memberId}", request);
        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutMemberReturnsBadRequestWhenEmailIsWhitespace()
    {

        int memberId = await AuthenticateAsMember();

        UpdateMemberRequest request = new()
        {
            Name = "Riyad",
            Email = "   "
        };

        var response = await Client.PutAsJsonAsync($"/members/{memberId}", request);
        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutMemberReturnsBadRequestWhenEmailHasNoAtSign()
    {
        int memberId = await AuthenticateAsMember();

        UpdateMemberRequest request = new()
        {
            Name = "Riyad",
            Email = "rgmail.com"
        };

        var response = await Client.PutAsJsonAsync($"/members/{memberId}", request);
        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutMemberReturnsConflictWhenEmailBelongsToAnotherMember()
    {

        int memberId = await AuthenticateAsMember();

        Writer.Seed(db =>
        {
            db.Members.Add(
                new Member
                {
                    Name = new MemberName("Mark"),
                    Email = new MemberEmail("mark@example.com"),
                    PasswordHash = "test-password-hash"
                });
        });

        UpdateMemberRequest request = new()
        {
            Name = "Riyad",
            Email = "mark@example.com"
        };

        var response = await Client.PutAsJsonAsync($"/members/{memberId}", request);
        await response.ShouldHaveStatusCode(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task PutMemberAllowsKeepingOwnEmail()
    {
        int memberId = await AuthenticateAsMember();

        UpdateMemberRequest request = new()
        {
            Name = "Riyad Salem",
            Email = "riyad.m.salem.19988@gmail.com"
        };

        var response = await Client.PutAsJsonAsync($"/members/{memberId}", request);
        await response.ShouldHaveStatusCode(HttpStatusCode.NoContent);
    }

}