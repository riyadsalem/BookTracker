using System.Text.Json;
using BookTracker.Api.Application.Members;
using BookTracker.Api.Domain;
using BookTracker.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace BookTracker.Api.Tests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task DomainExceptionReturnsBadRequest() // 400
    {
        DomainException exception = new("Invalid book data.");
        var context = await Execute(exception);

        Assert.Equal(
            StatusCodes.Status400BadRequest,
            context.Response.StatusCode);

        var response = await ReadErrorResponse(context);
        Assert.Equal("Invalid book data.", response!.Error);
    }

    [Fact]
    public async Task MemberEmailAlreadyExistsExceptionReturnsConflict() // 409
    {
        MemberEmailAlreadyExistsException exception = new();
        var context = await Execute(exception);

        Assert.Equal(
            StatusCodes.Status409Conflict,
            context.Response.StatusCode);

        var response = await ReadErrorResponse(context);
        Assert.Equal(exception.Message, response!.Error);
    }

    [Fact]
    public async Task ForbiddenExceptionReturns403() // 403
    {
        ForbiddenOperationException exception = new("Not allowed.");
        var context = await Execute(exception);

        Assert.Equal(
            StatusCodes.Status403Forbidden,
            context.Response.StatusCode);

        var response = await ReadErrorResponse(context);
        Assert.Equal("Not allowed.", response!.Error);
    }

    [Fact]
    public async Task UnexpectedExceptionReturns500() // 500
    {
        InvalidOperationException exception = new("Connection string: super-secret-details");
        var context = await Execute(exception);

        Assert.Equal(
            StatusCodes.Status500InternalServerError,
            context.Response.StatusCode);

        var response = await ReadErrorResponse(context);

        Assert.Equal("An unexpected error occurred.", response!.Error); // van ExceptionHandlingMiddleware
        Assert.DoesNotContain("super-secret-details", response.Error); // DOESNOTCONTAIN
    }

    private static async Task<DefaultHttpContext> Execute(Exception exception)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionHandlingMiddleware(
            _ => throw exception,
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);
        return context;
    }

    private static async Task<ErrorResponse?> ReadErrorResponse(DefaultHttpContext context)
    {
        context.Response.Body.Position = 0;
        return await JsonSerializer.DeserializeAsync<ErrorResponse>(
            context.Response.Body,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }
}