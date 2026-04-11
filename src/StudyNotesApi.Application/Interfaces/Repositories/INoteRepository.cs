using StudyNotesApi.Application.Models.Common;
using StudyNotesApi.Application.Models.Notes;
using StudyNotesApi.Domain.Entities;

namespace StudyNotesApi.Application.Interfaces.Repositories;

public interface INoteRepository
{
    Task AddAsync(Note note, CancellationToken cancellationToken = default);

    Task<Note?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    Task<PagedResult<Note>> GetByUserAsync(
        Guid userId,
        NoteFilter filter,
        PagedQuery pagedQuery,
        SortRequest sortRequest,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(Note note, CancellationToken cancellationToken = default);
}
