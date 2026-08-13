using System.Net;
using System.Net.Http.Json;
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
        response.EnsureSuccessStatusCode(); // Ensure ((Throw Exception)) [401(Unauthorized),403(Forbidden),400(Bad Request),500(Internal Server Error)]
        return await response.Content.ReadFromJsonAsync<BookDetailsResponse>() ?? throw new InvalidOperationException("Book details response was empty.");
    }
}