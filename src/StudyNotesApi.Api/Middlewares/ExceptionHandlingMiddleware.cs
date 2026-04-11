using System.Text.Json;
using StudyNotesApi.Api.DTOs.Common;
using StudyNotesApi.Application.Common.Exceptions;

namespace StudyNotesApi.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, error) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "ValidationError"),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden"),
            NotFoundException => (StatusCodes.Status404NotFound, "NotFound"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (StatusCodes.Status500InternalServerError, "InternalServerError")
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new ApiErrorResponse(
            StatusCode: statusCode,
            Error: error,
            Message: exception.Message,
            Path: context.Request.Path,
            TimestampUtc: DateTime.UtcNow,
            TraceId: context.TraceIdentifier);

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
