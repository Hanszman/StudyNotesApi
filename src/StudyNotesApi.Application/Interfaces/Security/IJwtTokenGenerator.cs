using StudyNotesApi.Application.Models.Auth;
using StudyNotesApi.Domain.Entities;

namespace StudyNotesApi.Application.Interfaces.Security;

public interface IJwtTokenGenerator
{
    AuthTokenResult GenerateToken(User user);
}
