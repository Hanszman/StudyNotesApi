using Microsoft.EntityFrameworkCore;
using StudyNotesApi.Application.Common.Exceptions;
using StudyNotesApi.Application.Interfaces.Repositories;
using StudyNotesApi.Application.Models.Common;
using StudyNotesApi.Application.Models.Tags;
using StudyNotesApi.Domain.Entities;
using StudyNotesApi.Infrastructure.Data.Context;

namespace StudyNotesApi.Infrastructure.Repositories;

public class TagRepository : ITagRepository
{
    private static readonly IReadOnlyDictionary<string, string> SortableFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["name"] = nameof(Tag.Name),
        ["createdat"] = nameof(Tag.CreatedAt),
        ["updatedat"] = nameof(Tag.UpdatedAt)
    };

    private readonly ApplicationDbContext _dbContext;

    public TagRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Tag tag, CancellationToken cancellationToken = default)
    {
        await _dbContext.Tags.AddAsync(tag, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(Guid userId, string name, Guid? excludeTagId = null, CancellationToken cancellationToken = default)
    {
        return _dbContext.Tags.AnyAsync(
            tag => tag.UserId == userId &&
                   tag.Name == name &&
                   (!excludeTagId.HasValue || tag.Id != excludeTagId.Value),
            cancellationToken);
    }

    public Task<Tag?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Tags
            .AsNoTracking()
            .FirstOrDefaultAsync(tag => tag.Id == id && tag.UserId == userId, cancellationToken);
    }

    public async Task<PagedResult<Tag>> GetByUserAsync(
        Guid userId,
        TagFilter filter,
        PagedQuery pagedQuery,
        SortRequest sortRequest,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Tags
            .AsNoTracking()
            .Where(tag => tag.UserId == userId);

        if (filter.Search is not null)
        {
            query = query.Where(tag => tag.Name.Contains(filter.Search));
        }

        query = ApplySorting(query, sortRequest);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(pagedQuery.Skip)
            .Take(pagedQuery.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Tag>(items, pagedQuery.PageNumber, pagedQuery.PageSize, totalCount);
    }

    public async Task<IReadOnlyCollection<Tag>> GetByIdsAsync(Guid userId, IReadOnlyCollection<Guid> tagIds, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tags
            .AsNoTracking()
            .Where(tag => tag.UserId == userId && tagIds.Contains(tag.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task RemoveAsync(Tag tag, CancellationToken cancellationToken = default)
    {
        _dbContext.Tags.Remove(tag);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<Tag> ApplySorting(IQueryable<Tag> query, SortRequest sortRequest)
    {
        var sortBy = sortRequest.SortBy ?? "createdAt";
        if (!SortableFields.TryGetValue(sortBy, out var propertyName))
        {
            throw new ValidationException($"Invalid sort field for tags: '{sortRequest.SortBy}'.");
        }

        return sortRequest.Direction == SortDirection.Asc
            ? query.OrderBy(tag => EF.Property<object?>(tag, propertyName))
            : query.OrderByDescending(tag => EF.Property<object?>(tag, propertyName));
    }
}
