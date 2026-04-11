namespace StudyNotesApi.Api.DTOs.Common;

public sealed record ApiErrorResponse(
    int StatusCode,
    string Error,
    string Message,
    string Path,
    DateTime TimestampUtc,
    string? TraceId);
