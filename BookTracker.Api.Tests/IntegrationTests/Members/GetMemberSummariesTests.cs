using System.Net;
using System.Net.Http.Json;
using BookTracker.Api.Application.Members.CreateMember;
using BookTracker.Api.Application.Members.GetMemberSummaries;
using BookTracker.Api.Domain.Members;

namespace BookTracker.Api.Tests.IntegrationTests.Members;

[Collection(PostgreSqlCollection.Name)]
public class GetMemberSummariesTests(PostgreSqlFixture database) : IntegrationTest(database)
{
    [Fact]
    public async Task GetMemberSummariesReturnsMembers()
    {
        await AuthenticateAsMember(MemberRole.Administrator);

        Writer.Seed(db =>
        {
            db.Members.Add(new Member
            {
                Name = new MemberName("Riyad"),
                Email = new MemberEmail("riyad@gmail.com"),
                PasswordHash = "123456789"
            });

            db.Members.Add(new Member
            {
                Name = new MemberName("Mark"),
                Email = new MemberEmail("mark@gmail.com"),
                PasswordHash = "123456789"
            });
        });

        var response = await Client.GetAsync("/members");

        var result =
            await response.ReadJsonAs<GetMemberSummariesResponse>(HttpStatusCode.OK);

        Assert.NotNull(result);

        Assert.Equal(3, result.TotalItems);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);

        Assert.Equal(3, result.Items.Count);

        Assert.Equal("Riyad", result.Items[1].Name);
        Assert.Equal("Mark", result.Items[2].Name);
    }

    [Fact]
    public async Task GetMemberSummariesCanSearchByName()
    {
        await AuthenticateAsMember(MemberRole.Administrator);

        Writer.Seed(db =>
        {
            db.Members.Add(new Member
            {
                Name = new MemberName("Riyad"),
                Email = new MemberEmail("r@gmail.com"),
                PasswordHash = "123456789"
            });

            db.Members.Add(new Member
            {
                Name = new MemberName("Mark"),
                Email = new MemberEmail("m@gmail.com"),
                PasswordHash = "123456789"
            });
        });


        var response = await Client.GetAsync("/members?search=riy");
        GetMemberSummariesResponse result = await response.ReadJsonAs<GetMemberSummariesResponse>(HttpStatusCode.OK);
        MemberSummary member = Assert.Single(result.Items);

        Assert.Equal("Riyad", member.Name);
    }

    [Fact]
    public async Task GetMemberSummariesCanSearchByEmail()
    {
        await AuthenticateAsMember(MemberRole.Administrator);

        Writer.Seed(db =>
        {
            db.Members.Add(new Member
            {
                Name = new MemberName("Riyad"),
                Email = new MemberEmail("r@gmail.com"),
                PasswordHash = "123456789"
            });

            db.Members.Add(new Member
            {
                Name = new MemberName("Mark"),
                Email = new MemberEmail("m@test.com"),
                PasswordHash = "123456789"
            });
        });

        var response = await Client.GetAsync("/members?search=test.com");
        GetMemberSummariesResponse result = await response.ReadJsonAs<GetMemberSummariesResponse>(HttpStatusCode.OK);
        MemberSummary member = Assert.Single(result.Items);

        Assert.Equal("Mark", member.Name);
    }

    [Fact]
    public async Task GetMemberSummariesApplyPagingAfterSearch()
    {
        await AuthenticateAsMember(MemberRole.Administrator);

        Writer.Seed(db =>
        {
            db.Members.Add(new Member
            {
                Name = new MemberName("Riyad"),
                Email = new MemberEmail("r@gmail.com"),
                PasswordHash = "123456789"
            });

            db.Members.Add(new Member
            {
                Name = new MemberName("Mark"),
                Email = new MemberEmail("m@gmail.com"),
                PasswordHash = "123456789"
            });

            db.Members.Add(new Member
            {
                Name = new MemberName("riyad"),
                Email = new MemberEmail("ri@gmail.com"),
                PasswordHash = "123456789"
            });
        });

        var response = await Client.GetAsync("/members?search=riy&page=1&pageSize=1");
        GetMemberSummariesResponse result = await response.ReadJsonAs<GetMemberSummariesResponse>(HttpStatusCode.OK);

        Assert.Single(result.Items);
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(1, result.Page);
        Assert.Equal(1, result.PageSize);
    }

    [Fact]
    public async Task SearchByPercentSignReturnsExactMatch()
    {
        await AuthenticateAsMember(MemberRole.Administrator);
        Writer.Seed(db =>
        {
            db.Members.Add(new Member
            {
                Name = new MemberName("1% Riyad"),
                Email = new MemberEmail("r@gmail.com"),
                PasswordHash = "123456789"
            });

            db.Members.Add(new Member
            {
                Name = new MemberName("Mark"),
                Email = new MemberEmail("m@test.com"),
                PasswordHash = "123456789"
            });
        });

        var response = await Client.GetAsync("/members?search=%25");
        GetMemberSummariesResponse result = await response.ReadJsonAs<GetMemberSummariesResponse>(HttpStatusCode.OK);

        MemberSummary member = Assert.Single(result.Items);
        Assert.Equal("1% Riyad", member.Name);
        Assert.Equal(1, result.TotalItems);
    }

    [Fact]
    public async Task SearchByUnderscoreReturnsExactMatch()
    {
        await AuthenticateAsMember(MemberRole.Administrator);
        Writer.Seed(db =>
        {
            db.Members.Add(new Member
            {
                Name = new MemberName("_ Riyad"),
                Email = new MemberEmail("r@gmail.com"),
                PasswordHash = "123456789"
            });

            db.Members.Add(new Member
            {
                Name = new MemberName("Mark"),
                Email = new MemberEmail("m@test.com"),
                PasswordHash = "123456789"
            });
        });

        var response = await Client.GetAsync("/members?search=_ Riyad");
        GetMemberSummariesResponse result = await response.ReadJsonAs<GetMemberSummariesResponse>(HttpStatusCode.OK);

        MemberSummary member = Assert.Single(result.Items);
        Assert.Equal("_ Riyad", member.Name);
        Assert.Equal(1, result.TotalItems);
    }

    /*
        [Fact] // SQLite
        public async Task SearchByStringTerminatorReturnsExactMatch()
        {
            await AuthenticateAsMember(MemberRole.Administrator);

            Writer.Seed(db =>
            {
                db.Members.AddRange(
                    new Member
                    {
                        Name = new MemberName("LessThan\0"),
                        Email = new MemberEmail("riyad@gmail.com"),
                        PasswordHash = "test-password-hash"
                    },
                    new Member
                    {
                        Name = new MemberName("THINKINGTEST"),
                        Email = new MemberEmail("mark@gmail.com"),
                        PasswordHash = "test-password-hash"
                    });
            });

            var response = await Client.GetAsync("/members?search=\0");
            GetMemberSummariesResponse result = await response.ReadJsonAs<GetMemberSummariesResponse>(HttpStatusCode.OK);

            MemberSummary member = Assert.Single(result.Items);
            Assert.Equal("LessThan\0", member.Name);
            Assert.Equal(1, result.TotalItems);
        }
    */

    [Fact]
    public async Task CreateMemberRejectsNameWithNullCharacter()
    {
        CreateMemberRequest request = new()
        {
            Name = "LessThan\0",
            Email = "riyad@gmail.com",
            Password = "analytical-engine"
        };

        var response = await Client.PostAsJsonAsync("/members", request);
        await response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }

}