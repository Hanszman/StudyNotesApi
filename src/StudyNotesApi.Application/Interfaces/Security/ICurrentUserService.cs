namespace StudyNotesApi.Application.Interfaces.Security;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }

    Guid GetRequiredUserId();
}
