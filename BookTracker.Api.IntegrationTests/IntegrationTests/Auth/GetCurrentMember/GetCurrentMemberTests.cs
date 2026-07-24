using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BookTracker.Api.Application.Auth.GetCurrentMember;
using BookTracker.Api.Application.Auth.Login;
using BookTracker.Api.Domain.Members;
using Microsoft.AspNetCore.Identity;

namespace BookTracker.Api.IntegrationTests.Auth.GetCurrentMember;

[Collection(PostgreSqlCollection.Name)]
public class GetCurrentMemberTests(PostgreSqlFixture database) : IntegrationTest(database)
{
    private void SeedMember(string password = "analytical-engine")
    {
        Member member = new()
        {
            Name = new MemberName("Ada Lovelace"),
            Email = new MemberEmail("ada@example.com"),
            PasswordHash = string.Empty
        };

        PasswordHasher<Member> passwordHasher = new();
        member.PasswordHash = passwordHasher.HashPassword(member, password);

        Writer.Seed(db => db.Members.Add(member));
    }

    [Fact]
    public async Task GetCurrentMemberRequiresAuthentication()
    {
        var response = await Client.GetAsync("/auth/me");
        await response.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentMemberReturnsTokenClaims()
    {
        SeedMember();

        LoginRequest loginRequest = new() { Email = "ada@example.com", Password = "analytical-engine" };
        var loginResponse = await Client.PostAsJsonAsync("/auth/login", loginRequest);
        var login = await loginResponse.ReadJsonAs<LoginResponse>(HttpStatusCode.OK);

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login.AccessToken);

        var response = await Client.GetAsync("/auth/me");
        CurrentMemberResponse member = await response.ReadJsonAs<CurrentMemberResponse>(HttpStatusCode.OK);

        Assert.Equal(1, member.Id);
        Assert.Equal("Ada Lovelace", member.Name);
        Assert.Equal("ada@example.com", member.Email);
    }

    [Fact]
    public async Task GetCurrentMemberRejectsInvalidToken()
    {
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "this-is-not-a-valid-token");

        var response = await Client.GetAsync("/auth/me");
        await response.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentMemberReturnsRole()
    {
        await AuthenticateAsMember(MemberRole.Administrator);

        var response = await Client.GetAsync("/auth/me");

        var member = await response.ReadJsonAs<CurrentMemberResponse>(HttpStatusCode.OK);

        Assert.Equal("Administrator", member.Role);
    }
}