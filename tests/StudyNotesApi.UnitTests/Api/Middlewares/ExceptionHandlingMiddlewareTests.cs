using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using StudyNotesApi.Api.DTOs.Common;
using StudyNotesApi.Api.Middlewares;
using StudyNotesApi.Application.Common.Exceptions;

namespace StudyNotesApi.UnitTests.Api.Middlewares;

public class ExceptionHandlingMiddlewareTests
{
    [Theory]
    [InlineData(typeof(ValidationException), 400, "ValidationError")]
    [InlineData(typeof(UnauthorizedException), 401, "Unauthorized")]
    [InlineData(typeof(ForbiddenException), 403, "Forbidden")]
    [InlineData(typeof(NotFoundException), 404, "NotFound")]
    [InlineData(typeof(ConflictException), 409, "Conflict")]
    public async Task InvokeAsync_should_map_known_exceptions_to_expected_status_codes(Type exceptionType, int statusCode, string error)
    {
        var context = CreateHttpContext();
        var middleware = new ExceptionHandlingMiddleware(_ => throw CreateException(exceptionType, "Handled error."));

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(statusCode);

        context.Response.Body.Position = 0;
        var response = await JsonSerializer.DeserializeAsync<ApiErrorResponse>(context.Response.Body);
        response.Should().NotBeNull();
        response!.Error.Should().Be(error);
        response.Message.Should().Be("Handled error.");
        response.Path.Should().Be("/api/test");
    }

    [Fact]
    public async Task InvokeAsync_should_return_internal_server_error_for_unknown_exceptions()
    {
        var context = CreateHttpContext();
        var middleware = new ExceptionHandlingMiddleware(_ => throw new InvalidOperationException("Unexpected error."));

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(500);

        context.Response.Body.Position = 0;
        var response = await JsonSerializer.DeserializeAsync<ApiErrorResponse>(context.Response.Body);
        response.Should().NotBeNull();
        response!.Error.Should().Be("InternalServerError");
    }

    [Fact]
    public async Task InvokeAsync_should_call_next_when_no_exception_is_thrown()
    {
        var context = CreateHttpContext();
        var nextWasCalled = false;
        var middleware = new ExceptionHandlingMiddleware(_ =>
        {
            nextWasCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        nextWasCalled.Should().BeTrue();
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        return new DefaultHttpContext
        {
            TraceIdentifier = "trace-id",
            Request =
            {
                Path = "/api/test"
            },
            Response =
            {
                Body = new MemoryStream()
            }
        };
    }

    private static Exception CreateException(Type type, string message)
    {
        return type.Name switch
        {
            nameof(ValidationException) => new ValidationException(message),
            nameof(UnauthorizedException) => new UnauthorizedException(message),
            nameof(ForbiddenException) => new ForbiddenException(message),
            nameof(NotFoundException) => new NotFoundException(message),
            nameof(ConflictException) => new ConflictException(message),
            _ => throw new InvalidOperationException("Unsupported exception type.")
        };
    }
}
