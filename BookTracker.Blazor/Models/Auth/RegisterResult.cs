namespace BookTracker.Blazor.Models.Auth;

public enum RegisterStatus
{
    Registered, // 201
    ValidationFailed, // Bad Request (400) valueObject Errors
    EmailAlreadyExists // Conflict (409)
}

public sealed record RegisterResult(RegisterStatus Status, string? ErrorMessage = null);