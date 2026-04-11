using StudyNotesApi.Application.Models.Common;
using StudyNotesApi.Application.Models.Tags;
using StudyNotesApi.Domain.Entities;

namespace StudyNotesApi.Application.Interfaces.Services;

public interface ITagService
{
    Task<PagedResult<Tag>> GetTagsAsync(
        Guid userId,
        TagFilter filter,
        PagedQuery pagedQuery,
        SortRequest sortRequest,
        CancellationToken cancellationToken = default);

    Task<Tag> GetByIdAsync(Guid userId, Guid tagId, CancellationToken cancellationToken = default);

    Task<Tag> CreateAsync(Guid userId, string name, CancellationToken cancellationToken = default);

    Task<Tag> UpdateAsync(Guid userId, Guid tagId, string name, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid userId, Guid tagId, CancellationToken cancellationToken = default);
}
