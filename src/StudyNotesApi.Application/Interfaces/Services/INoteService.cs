using StudyNotesApi.Application.Models.Common;
using StudyNotesApi.Application.Models.Notes;
using StudyNotesApi.Domain.Entities;

namespace StudyNotesApi.Application.Interfaces.Services;

public interface INoteService
{
    Task<PagedResult<Note>> GetNotesAsync(
        Guid userId,
        NoteFilter filter,
        PagedQuery pagedQuery,
        SortRequest sortRequest,
        CancellationToken cancellationToken = default);

    Task<Note> GetByIdAsync(Guid userId, Guid noteId, CancellationToken cancellationToken = default);

    Task<Note> CreateAsync(
        Guid userId,
        string title,
        string content,
        Guid? categoryId,
        IReadOnlyCollection<Guid> tagIds,
        bool isFavorite,
        bool isArchived,
        bool isPinned,
        CancellationToken cancellationToken = default);

    Task<Note> UpdateAsync(
        Guid userId,
        Guid noteId,
        string title,
        string content,
        Guid? categoryId,
        IReadOnlyCollection<Guid> tagIds,
        bool isFavorite,
        bool isArchived,
        bool isPinned,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid userId, Guid noteId, CancellationToken cancellationToken = default);
}
