using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BookTracker.Blazor.Models.Auth;
using BookTracker.Blazor.Models.Books;
using BookTracker.Blazor.Models.Members;

namespace BookTracker.Blazor.Api;

public sealed class BookTrackerClient(HttpClient httpClient)
{
    // BOOKS .....
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

    public async Task<CreateBookResult> CreateBook(CreateBookRequest request)
    {
        using var response = await httpClient.PostAsJsonAsync("/books", request);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            return new CreateBookResult(CreateBookStatus.Unauthorized);

        if (response.StatusCode == HttpStatusCode.Forbidden)
            return new CreateBookResult(CreateBookStatus.Forbidden);

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            string? message = await TryReadErrorMessage(response);
            return new CreateBookResult(CreateBookStatus.ValidationFailed,
                ErrorMessage: message ?? "De opgegeven boekgegevens zijn ongeldig.");
        }

        response.EnsureSuccessStatusCode();

        CreateBookResponse book = await response.Content.ReadFromJsonAsync<CreateBookResponse>() ?? throw new InvalidOperationException("Create book response was empty.");

        return new CreateBookResult(CreateBookStatus.Created, book);
    }

    public async Task<UpdateBookResult> UpdateBook(int id, UpdateBookRequest request)
    {
        var response = await httpClient.PutAsJsonAsync($"/books/{id}", request);

        if (response.StatusCode == HttpStatusCode.NoContent)
            return new UpdateBookResult(UpdateBookStatus.Updated);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            return new UpdateBookResult(UpdateBookStatus.Unauthorized);

        if (response.StatusCode == HttpStatusCode.Forbidden)
            return new UpdateBookResult(UpdateBookStatus.Forbidden);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return new UpdateBookResult(UpdateBookStatus.NotFound);

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            string? conflictMessage = await TryReadErrorMessage(response);
            return new UpdateBookResult(UpdateBookStatus.Conflict,
                conflictMessage ?? "The book was changed by another user.");
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            string? validationMessage = await TryReadErrorMessage(response);
            return new UpdateBookResult(UpdateBookStatus.ValidationFailed,
                validationMessage ?? "De opgegeven boekgegevens zijn ongeldig.");
        }

        response.EnsureSuccessStatusCode();
        throw new InvalidOperationException($"Unexpected status code {response.StatusCode} from PUT /books/{id}.");
    }
    public async Task<DeleteBookResult> DeleteBook(int id)
    {
        var response = await httpClient.DeleteAsync($"/books/{id}");

        if (response.StatusCode == HttpStatusCode.NoContent)
            return new DeleteBookResult(DeleteBookStatus.Deleted);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            return new DeleteBookResult(DeleteBookStatus.Unauthorized);

        if (response.StatusCode == HttpStatusCode.Forbidden)
            return new DeleteBookResult(DeleteBookStatus.Forbidden);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return new DeleteBookResult(DeleteBookStatus.NotFound);

        response.EnsureSuccessStatusCode();

        throw new InvalidOperationException($"Unexpected status code {response.StatusCode} from DELETE /books/{id}.");
    }
    public async Task<RegisterResult> Register(RegisterRequest request)
    {
        using var response = await httpClient.PostAsJsonAsync("/members", request);

        if (response.StatusCode == HttpStatusCode.Conflict)
            return new RegisterResult(RegisterStatus.EmailAlreadyExists, "Er bestaat al een account met dit e-mailadres.");

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            string? message = await TryReadErrorMessage(response);
            return new RegisterResult(RegisterStatus.ValidationFailed,
                message ?? "De opgegeven gegevens zijn ongeldig.");
        }

        response.EnsureSuccessStatusCode();
        return new RegisterResult(RegisterStatus.Registered);
    }

    // MEMBERS .........
    public async Task<CurrentMemberResponse?> GetCurrentMember()
    {
        var response = await httpClient.GetAsync("/auth/me");

        if (response.StatusCode == HttpStatusCode.Unauthorized) return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CurrentMemberResponse>();
    }

    public async Task<GetMemberSummariesResponse> GetMembers(string? search, int page, int pageSize)
    {
        string url = $"/members?page={page}&pageSize={pageSize}";

        if (!string.IsNullOrWhiteSpace(search)) url += $"&search={Uri.EscapeDataString(search)}";

        return await httpClient.GetFromJsonAsync<GetMemberSummariesResponse>(url)
            ?? throw new InvalidOperationException("Member list response was empty.");
    }

    public async Task<MemberDetailsResponse?> GetMemberDetails(int id)
    {
        var response = await httpClient.GetAsync($"/members/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound) return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MemberDetailsResponse>();
    }

    public async Task<UpdateMemberResult> UpdateMember(int id, UpdateMemberRequest request)
    {
        using var response = await httpClient.PutAsJsonAsync($"/members/{id}", request);

        if (response.StatusCode == HttpStatusCode.NoContent)
            return new UpdateMemberResult(UpdateMemberStatus.Updated);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            return new UpdateMemberResult(UpdateMemberStatus.Unauthorized);

        if (response.StatusCode == HttpStatusCode.Forbidden)
            return new UpdateMemberResult(UpdateMemberStatus.Forbidden);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return new UpdateMemberResult(UpdateMemberStatus.NotFound);

        if (response.StatusCode == HttpStatusCode.Conflict)
            return new UpdateMemberResult(UpdateMemberStatus.EmailConflict,
        "Dit e-mailadres wordt al door een andere gebruiker gebruikt.");

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            string? message = await TryReadErrorMessage(response);
            return new UpdateMemberResult(UpdateMemberStatus.ValidationFailed,
                message ?? "De opgegeven gegevens zijn ongeldig.");
        }

        response.EnsureSuccessStatusCode();
        throw new InvalidOperationException($"Unexpected status {response.StatusCode}.");

    }

    public async Task<DeleteMemberResult> DeleteMember(int id)
    {
        var response = await httpClient.DeleteAsync($"/members/{id}");

        if (response.StatusCode == HttpStatusCode.NoContent)
            return new DeleteMemberResult(DeleteMemberStatus.Deleted);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            return new DeleteMemberResult(DeleteMemberStatus.Unauthorized);

        if (response.StatusCode == HttpStatusCode.Forbidden)
            return new DeleteMemberResult(DeleteMemberStatus.Forbidden);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return new DeleteMemberResult(DeleteMemberStatus.NotFound);

        response.EnsureSuccessStatusCode();
        throw new InvalidOperationException($"Unexpected status {response.StatusCode}.");
    }

    private static async Task<string?> TryReadErrorMessage(HttpResponseMessage response)
    {
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            return error?.Error;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}