using StudyNotesApi.Application.Common.Exceptions;
using StudyNotesApi.Application.Interfaces.Repositories;
using StudyNotesApi.Application.Interfaces.Services;
using StudyNotesApi.Application.Models.Categories;
using StudyNotesApi.Application.Models.Common;
using StudyNotesApi.Domain.Entities;

namespace StudyNotesApi.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public Task<PagedResult<Category>> GetCategoriesAsync(
        Guid userId,
        CategoryFilter filter,
        PagedQuery pagedQuery,
        SortRequest sortRequest,
        CancellationToken cancellationToken = default)
    {
        return _categoryRepository.GetByUserAsync(userId, filter, pagedQuery, sortRequest, cancellationToken);
    }

    public async Task<Category> GetByIdAsync(Guid userId, Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await GetRequiredCategoryAsync(userId, categoryId, cancellationToken);
    }

    public async Task<Category> CreateAsync(Guid userId, string name, string? color, CancellationToken cancellationToken = default)
    {
        var normalizedName = ValidateRequiredName(name);

        if (await _categoryRepository.ExistsByNameAsync(userId, normalizedName, null, cancellationToken))
        {
            throw new ConflictException("A category with this name already exists.");
        }

        var category = new Category(normalizedName, userId, color);
        await _categoryRepository.AddAsync(category, cancellationToken);

        return category;
    }

    public async Task<Category> UpdateAsync(Guid userId, Guid categoryId, string name, string? color, CancellationToken cancellationToken = default)
    {
        var category = await GetRequiredCategoryAsync(userId, categoryId, cancellationToken);
        var normalizedName = ValidateRequiredName(name);

        if (await _categoryRepository.ExistsByNameAsync(userId, normalizedName, categoryId, cancellationToken))
        {
            throw new ConflictException("A category with this name already exists.");
        }

        category.Rename(normalizedName);
        category.UpdateColor(color);

        await _categoryRepository.UpdateAsync(category, cancellationToken);
        return category;
    }

    public async Task DeleteAsync(Guid userId, Guid categoryId, CancellationToken cancellationToken = default)
    {
        var category = await GetRequiredCategoryAsync(userId, categoryId, cancellationToken);
        await _categoryRepository.RemoveAsync(category, cancellationToken);
    }

    private async Task<Category> GetRequiredCategoryAsync(Guid userId, Guid categoryId, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId, userId, cancellationToken);
        if (category is null)
        {
            throw new NotFoundException("Category was not found.");
        }

        return category;
    }

    private static string ValidateRequiredName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException("Category name is required.");
        }

        return name.Trim();
    }
}
