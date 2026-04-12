using Microsoft.EntityFrameworkCore;
using StudyNotesApi.Application.Common.Exceptions;
using StudyNotesApi.Application.Interfaces.Repositories;
using StudyNotesApi.Application.Models.Common;
using StudyNotesApi.Application.Models.Notes;
using StudyNotesApi.Domain.Entities;
using StudyNotesApi.Infrastructure.Data.Context;

namespace StudyNotesApi.Infrastructure.Repositories;

public class NoteRepository : INoteRepository
{
    private static readonly IReadOnlyDictionary<string, string> SortableFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["title"] = nameof(Note.Title),
        ["content"] = nameof(Note.Content),
        ["isfavorite"] = nameof(Note.IsFavorite),
        ["isarchived"] = nameof(Note.IsArchived),
        ["ispinned"] = nameof(Note.IsPinned),
        ["createdat"] = nameof(Note.CreatedAt),
        ["updatedat"] = nameof(Note.UpdatedAt)
    };

    private readonly ApplicationDbContext _dbContext;

    public NoteRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Note note, CancellationToken cancellationToken = default)
    {
        await _dbContext.Notes.AddAsync(note, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Note?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Notes
            .AsNoTracking()
            .Include(note => note.NoteTags)
            .FirstOrDefaultAsync(note => note.Id == id && note.UserId == userId, cancellationToken);
    }

    public async Task<PagedResult<Note>> GetByUserAsync(
        Guid userId,
        NoteFilter filter,
        PagedQuery pagedQuery,
        SortRequest sortRequest,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Notes
            .AsNoTracking()
            .Include(note => note.NoteTags)
            .Where(note => note.UserId == userId);

        if (filter.Search is not null)
        {
            query = query.Where(note => note.Title.Contains(filter.Search) || note.Content.Contains(filter.Search));
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(note => note.CategoryId == filter.CategoryId.Value);
        }

        if (filter.TagId.HasValue)
        {
            query = query.Where(note => note.NoteTags.Any(noteTag => noteTag.TagId == filter.TagId.Value));
        }

        if (filter.IsFavorite.HasValue)
        {
            query = query.Where(note => note.IsFavorite == filter.IsFavorite.Value);
        }

        if (filter.IsArchived.HasValue)
        {
            query = query.Where(note => note.IsArchived == filter.IsArchived.Value);
        }

        if (filter.IsPinned.HasValue)
        {
            query = query.Where(note => note.IsPinned == filter.IsPinned.Value);
        }

        query = ApplySorting(query, sortRequest);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(pagedQuery.Skip)
            .Take(pagedQuery.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Note>(items, pagedQuery.PageNumber, pagedQuery.PageSize, totalCount);
    }

    public async Task RemoveAsync(Note note, CancellationToken cancellationToken = default)
    {
        _dbContext.Notes.Remove(note);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Note note, CancellationToken cancellationToken = default)
    {
        var trackedNote = await _dbContext.Notes
            .Include(current => current.NoteTags)
            .FirstAsync(current => current.Id == note.Id && current.UserId == note.UserId, cancellationToken);

        _dbContext.Entry(trackedNote).CurrentValues.SetValues(note);

        var desiredTagIds = note.NoteTags.Select(noteTag => noteTag.TagId).ToHashSet();
        var currentTagIds = trackedNote.NoteTags.Select(noteTag => noteTag.TagId).ToHashSet();

        var noteTagsToRemove = trackedNote.NoteTags
            .Where(noteTag => !desiredTagIds.Contains(noteTag.TagId))
            .ToList();

        foreach (var noteTag in noteTagsToRemove)
        {
            _dbContext.NoteTags.Remove(noteTag);
        }

        foreach (var tagId in desiredTagIds.Except(currentTagIds))
        {
            _dbContext.NoteTags.Add(new NoteTag(trackedNote.Id, tagId));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<Note> ApplySorting(IQueryable<Note> query, SortRequest sortRequest)
    {
        var sortBy = sortRequest.SortBy ?? "createdAt";
        if (!SortableFields.TryGetValue(sortBy, out var propertyName))
        {
            throw new ValidationException($"Invalid sort field for notes: '{sortRequest.SortBy}'.");
        }

        return sortRequest.Direction == SortDirection.Asc
            ? query.OrderBy(note => EF.Property<object?>(note, propertyName))
            : query.OrderByDescending(note => EF.Property<object?>(note, propertyName));
    }
}
