using StudyNotesApi.Application.Models.Categories;
using StudyNotesApi.Application.Models.Common;
using StudyNotesApi.Domain.Entities;

namespace StudyNotesApi.Application.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task AddAsync(Category category, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(Guid userId, string name, Guid? excludeCategoryId = null, CancellationToken cancellationToken = default);

    Task<Category?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    Task<PagedResult<Category>> GetByUserAsync(
        Guid userId,
        CategoryFilter filter,
        PagedQuery pagedQuery,
        SortRequest sortRequest,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(Category category, CancellationToken cancellationToken = default);

    Task RemoveAsync(Category category, CancellationToken cancellationToken = default);
}
