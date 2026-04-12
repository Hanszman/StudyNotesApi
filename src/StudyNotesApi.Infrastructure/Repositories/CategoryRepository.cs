using Microsoft.EntityFrameworkCore;
using StudyNotesApi.Application.Common.Exceptions;
using StudyNotesApi.Application.Interfaces.Repositories;
using StudyNotesApi.Application.Models.Categories;
using StudyNotesApi.Application.Models.Common;
using StudyNotesApi.Domain.Entities;
using StudyNotesApi.Infrastructure.Data.Context;

namespace StudyNotesApi.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private static readonly IReadOnlyDictionary<string, string> SortableFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["name"] = nameof(Category.Name),
        ["color"] = nameof(Category.Color),
        ["createdat"] = nameof(Category.CreatedAt),
        ["updatedat"] = nameof(Category.UpdatedAt)
    };

    private readonly ApplicationDbContext _dbContext;

    public CategoryRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        await _dbContext.Categories.AddAsync(category, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(Guid userId, string name, Guid? excludeCategoryId = null, CancellationToken cancellationToken = default)
    {
        return _dbContext.Categories.AnyAsync(
            category => category.UserId == userId &&
                        category.Name == name &&
                        (!excludeCategoryId.HasValue || category.Id != excludeCategoryId.Value),
            cancellationToken);
    }

    public Task<Category?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(category => category.Id == id && category.UserId == userId, cancellationToken);
    }

    public async Task<PagedResult<Category>> GetByUserAsync(
        Guid userId,
        CategoryFilter filter,
        PagedQuery pagedQuery,
        SortRequest sortRequest,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Categories
            .AsNoTracking()
            .Where(category => category.UserId == userId);

        if (filter.Search is not null)
        {
            query = query.Where(category => category.Name.Contains(filter.Search));
        }

        query = ApplySorting(query, sortRequest);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(pagedQuery.Skip)
            .Take(pagedQuery.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Category>(items, pagedQuery.PageNumber, pagedQuery.PageSize, totalCount);
    }

    public async Task RemoveAsync(Category category, CancellationToken cancellationToken = default)
    {
        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Category category, CancellationToken cancellationToken = default)
    {
        _dbContext.Categories.Update(category);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<Category> ApplySorting(IQueryable<Category> query, SortRequest sortRequest)
    {
        var sortBy = sortRequest.SortBy ?? "createdAt";
        if (!SortableFields.TryGetValue(sortBy, out var propertyName))
        {
            throw new ValidationException($"Invalid sort field for categories: '{sortRequest.SortBy}'.");
        }

        return sortRequest.Direction == SortDirection.Asc
            ? query.OrderBy(category => EF.Property<object?>(category, propertyName))
            : query.OrderByDescending(category => EF.Property<object?>(category, propertyName));
    }
}
