using System.Net;
using BookTracker.Api.Application.Members.GetMemberSummaries;
using BookTracker.Api.Domain.Members;

namespace BookTracker.Api.Tests.IntegrationTests.Members;

public class GetMemberSummariesTests : IntegrationTest
{
    [Fact]
    public async Task GetMemberSummariesReturnsMembers()
    {
        Writer.Seed(db =>
        {
            db.Members.Add(new Member
            {
                Name = new MemberName("Riyad"),
                Email = new MemberEmail("riyad@gmail.com")
            });

            db.Members.Add(new Member
            {
                Name = new MemberName("Mark"),
                Email = new MemberEmail("mark@gmail.com")
            });
        });

        var response = await Client.GetAsync("/members");

        var result =
            await response.ReadJsonAs<GetMemberSummariesResponse>(HttpStatusCode.OK);

        Assert.NotNull(result);

        Assert.Equal(2, result.TotalItems);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);

        Assert.Equal(2, result.Items.Count);

        Assert.Equal("Riyad", result.Items[0].Name);
        Assert.Equal("Mark", result.Items[1].Name);
    }

    [Fact]
    public async Task GetMemberSummariesCanSearchByName()
    {
        Writer.Seed(db =>
        {
            db.Members.Add(new Member
            {
                Name = new MemberName("Riyad"),
                Email = new MemberEmail("r@gmail.com")
            });

            db.Members.Add(new Member
            {
                Name = new MemberName("Mark"),
                Email = new MemberEmail("m@gmail.com")
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
        Writer.Seed(db =>
        {
            db.Members.Add(new Member
            {
                Name = new MemberName("Riyad"),
                Email = new MemberEmail("r@gmail.com")
            });

            db.Members.Add(new Member
            {
                Name = new MemberName("Mark"),
                Email = new MemberEmail("m@test.com")
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
        Writer.Seed(db =>
        {
            db.Members.Add(new Member
            {
                Name = new MemberName("Riyad"),
                Email = new MemberEmail("r@gmail.com")
            });

            db.Members.Add(new Member
            {
                Name = new MemberName("Mark"),
                Email = new MemberEmail("m@gmail.com")
            });

            db.Members.Add(new Member
            {
                Name = new MemberName("riyad"),
                Email = new MemberEmail("ri@gmail.com")
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

}