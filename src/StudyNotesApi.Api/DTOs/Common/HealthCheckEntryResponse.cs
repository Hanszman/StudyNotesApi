namespace StudyNotesApi.Api.DTOs.Common;

public sealed record HealthCheckEntryResponse(
    string Status,
    string? Description,
    string Duration);
