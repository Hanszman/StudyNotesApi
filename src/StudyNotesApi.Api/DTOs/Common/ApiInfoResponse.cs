namespace StudyNotesApi.Api.DTOs.Common;

public sealed record ApiInfoResponse(
    string Name,
    string Version,
    string Environment,
    string DocumentationUrl,
    string HealthUrl,
    DateTime TimestampUtc);
