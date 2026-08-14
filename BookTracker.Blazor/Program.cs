using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BookTracker.Blazor;
using BookTracker.Blazor.Api;
using BookTracker.Blazor.Auth;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("ApiBaseUrl is missing.");

builder.Services.AddScoped<IAuthSession, AuthSession>();
builder.Services.AddTransient<AuthorizationMessageHandler>();

builder.Services.AddScoped(serviceProvider =>
{
    var authorizationHandler = serviceProvider
        .GetRequiredService<AuthorizationMessageHandler>();
    authorizationHandler.InnerHandler = new HttpClientHandler();

    return new HttpClient(authorizationHandler)
    {
        BaseAddress = new Uri(apiBaseUrl)
    };
});

builder.Services.AddScoped<BookTrackerClient>();


// this is a built in function from Blazor ... Zoals (I am sayint >> I want to enable the Authorization system)
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState(); // make user info that coms from AuthenticationStateProvider available to Blazor components

builder.Services.AddScoped<BookTrackerAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(serviceProvider => serviceProvider.GetRequiredService<BookTrackerAuthenticationStateProvider>());

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

await builder.Build().RunAsync();