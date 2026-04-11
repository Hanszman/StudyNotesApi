using StudyNotesApi.Application.Models.Categories;
using StudyNotesApi.Application.Models.Common;
using StudyNotesApi.Domain.Entities;

namespace StudyNotesApi.Application.Interfaces.Services;

public interface ICategoryService
{
    Task<PagedResult<Category>> GetCategoriesAsync(
        Guid userId,
        CategoryFilter filter,
        PagedQuery pagedQuery,
        SortRequest sortRequest,
        CancellationToken cancellationToken = default);

    Task<Category> GetByIdAsync(Guid userId, Guid categoryId, CancellationToken cancellationToken = default);

    Task<Category> CreateAsync(Guid userId, string name, string? color, CancellationToken cancellationToken = default);

    Task<Category> UpdateAsync(Guid userId, Guid categoryId, string name, string? color, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid userId, Guid categoryId, CancellationToken cancellationToken = default);
}
