namespace StudyNotesApi.Api.DTOs.Common;

public sealed record HealthResponse(
    string Status,
    string Environment,
    string ApiVersion,
    DateTime TimestampUtc,
    IReadOnlyDictionary<string, HealthCheckEntryResponse> Checks);
