using System.Net;
using System.Net.Http.Json;
using Bunit;
using BookTracker.Blazor.Api;
using BookTracker.Blazor.Models.Members;
using BookTracker.Blazor.Pages.Members;
using BookTracker.Blazor.Layout;
using Microsoft.Extensions.DependencyInjection;

namespace BookTracker.Blazor.Tests.Auth;

public class AuthorizationMatrixTests : BunitContext
{
    private sealed class FakeHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => await respond(request);
    }

    private void RegisterClient(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond)
    {
        var httpClient = new HttpClient(new FakeHandler(respond)) { BaseAddress = new Uri("http://localhost") };
        Services.AddSingleton(new BookTrackerClient(httpClient));
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("Member", false)]
    [InlineData("Administrator", true)]
    public void NavMenu_ShowsMembersLink_OnlyForAdministrator(string? role, bool shouldSeeLink)
    {
        var authorization = AddAuthorization();
        if (role is null) authorization.SetNotAuthorized();
        else
        {
            authorization.SetAuthorized("Test User");
            authorization.SetRoles(role);
        }

        var cut = Render<NavMenu>();
        Assert.Equal(shouldSeeLink, cut.Markup.Contains("Leden"));
    }

    [Fact]
    public void MemberList_Returns403ForRegularMember_DoesNotShowData()
    {
        RegisterClient(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Forbidden)));
        var cut = Render<MemberList>();
        Assert.DoesNotContain("<li>", cut.Markup);
    }

    [Fact]
    public void MemberList_Returns200ForAdministrator_ShowsData()
    {
        var authorization = AddAuthorization();
        authorization.SetAuthorized("Book Tracker Admin");
        authorization.SetRoles("Administrator");

        RegisterClient(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new GetMemberSummariesResponse
            {
                Items = [new MemberSummary { Id = 1, Name = "Riyad", Email = "riyad1998@bookTracker.com" }],
                Page = 1,
                PageSize = 10,
                TotalItems = 1,
                TotalPages = 1
            })
        }));
        var cut = Render<MemberList>();
        Assert.Contains("Riyad", cut.Markup);
    }
}