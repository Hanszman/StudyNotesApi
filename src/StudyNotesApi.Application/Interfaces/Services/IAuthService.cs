using StudyNotesApi.Application.Models.Auth;
using StudyNotesApi.Domain.Entities;

namespace StudyNotesApi.Application.Interfaces.Services;

public interface IAuthService
{
    Task<User> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);

    Task<AuthTokenResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
