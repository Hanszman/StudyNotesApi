namespace StudyNotesApi.Api.DTOs.Categories;

public sealed record CategoryResponseDto(
    Guid Id,
    string Name,
    string? Color,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
