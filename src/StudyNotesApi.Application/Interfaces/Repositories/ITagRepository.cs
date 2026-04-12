using StudyNotesApi.Application.Models.Common;
using StudyNotesApi.Application.Models.Tags;
using StudyNotesApi.Domain.Entities;

namespace StudyNotesApi.Application.Interfaces.Repositories;

public interface ITagRepository
{
    Task AddAsync(Tag tag, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(Guid userId, string name, Guid? excludeTagId = null, CancellationToken cancellationToken = default);

    Task<Tag?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    Task<PagedResult<Tag>> GetByUserAsync(
        Guid userId,
        TagFilter filter,
        PagedQuery pagedQuery,
        SortRequest sortRequest,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Tag>> GetByIdsAsync(Guid userId, IReadOnlyCollection<Guid> tagIds, CancellationToken cancellationToken = default);

    Task UpdateAsync(Tag tag, CancellationToken cancellationToken = default);

    Task RemoveAsync(Tag tag, CancellationToken cancellationToken = default);
}
