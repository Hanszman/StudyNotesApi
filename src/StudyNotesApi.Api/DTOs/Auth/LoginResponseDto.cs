namespace StudyNotesApi.Api.DTOs.Auth;

public sealed record LoginResponseDto(string AccessToken, int ExpiresIn, string TokenType);
