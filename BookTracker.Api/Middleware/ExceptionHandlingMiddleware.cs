using BookTracker.Api.Application.Members;
using BookTracker.Api.Domain;

namespace BookTracker.Api.Middleware;

public class ExceptionHandlingMiddleware(
    // RequestDelegate, ILogger >> van ASP.NET
    // ILogger >> schrijf de error in LOG (server)
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException)
            when (context.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            await WriteErrorResponse(context, exception);
        }
    }

    private async Task WriteErrorResponse(
        HttpContext context,
        Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            MemberEmailAlreadyExistsException =>
                (StatusCodes.Status409Conflict, exception.Message),

            ForbiddenOperationException =>
                (StatusCodes.Status403Forbidden, exception.Message),

            DomainException =>
                (StatusCodes.Status400BadRequest, exception.Message),

            _ =>
                (StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "An unhandled exception occurred while processing {Method} {Path}",
                context.Request.Method,
                context.Request.Path);
        }

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new ErrorResponse(message));
        /*
        alse de message is (Email alreade exists)
        user haal ({"message":"Email already exists"})
        */
    }
}