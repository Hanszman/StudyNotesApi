namespace StudyNotesApi.Application.Models.Auth;

public sealed record RegisterUserRequest(string Name, string Email, string Password);
