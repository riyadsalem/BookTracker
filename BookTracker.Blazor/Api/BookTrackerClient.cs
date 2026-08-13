using System.Net;
using System.Net.Http.Json;
using BookTracker.Blazor.Models.Auth;
using BookTracker.Blazor.Models.Books;

namespace BookTracker.Blazor.Api;

public sealed class BookTrackerClient(HttpClient httpClient)
{
    public async Task<GetBookSummariesResponse> GetBooks(string? search, int page, int pageSize)
    {
        string url = $"/books?page={page}&pageSize={pageSize}";

        if (!string.IsNullOrWhiteSpace(search)) url += $"&search={Uri.EscapeDataString(search)}";

        return await httpClient.GetFromJsonAsync<GetBookSummariesResponse>(url)
            ?? throw new InvalidOperationException("Book list response was empty.");
    }
    public async Task<BookDetailsResponse?> GetBookDetails(int id)
    {
        var response = await httpClient.GetAsync($"/books/{id}");
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BookDetailsResponse>() ?? throw new InvalidOperationException("Book details response was empty.");
    }

    /*
    IMPORTANT
    1 => the user enters their email and password in the form
    2 => they click "LOGIN"
    3 => Blazor sends a POST request to the API using (postAsJsonAsync)
    4 => the API checks the credentials against the database....
    5 => If the credentials are correct (The API returns a JWT token as JSON...)
    6 => Blazor receives the response and converts it into a C# object using (ReadFromJsonAsync)
    7 => Blazor stores the token in localStorage through AuthSession (pages/Auth/Login.razor)
    */
    public async Task<LoginResponse?> Login(LoginRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("/auth/login", request);
        // OBJECT TOOOOO (ex) >>> PostAsJsonAsycn ((({ "email": "admin@booktracker.local", "password": "admin12345.." })))

        if (response.StatusCode == HttpStatusCode.Unauthorized) return null;

        response.EnsureSuccessStatusCode(); // Ensure ((Throw Exception)) [401(Unauthorized),403(Forbidden),400(Bad Request),500(Internal Server Error)]

        return await response.Content.ReadFromJsonAsync<LoginResponse>() ?? throw new InvalidOperationException("Login response was empty.");
        // ReadFromJsonAsync<LoginResponse>() (ex) >>> { "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...", "expiresAt": "2026-08-13T15:30:00Z" }

    }
}