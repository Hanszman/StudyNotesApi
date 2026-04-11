namespace StudyNotesApi.Application.Models.Auth;

public sealed record AuthTokenResult(string AccessToken, int ExpiresIn, string TokenType = "Bearer");
