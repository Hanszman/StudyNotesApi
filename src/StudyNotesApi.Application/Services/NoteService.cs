using StudyNotesApi.Application.Common.Exceptions;
using StudyNotesApi.Application.Interfaces.Repositories;
using StudyNotesApi.Application.Interfaces.Services;
using StudyNotesApi.Application.Models.Common;
using StudyNotesApi.Application.Models.Notes;
using StudyNotesApi.Domain.Entities;

namespace StudyNotesApi.Application.Services;

public class NoteService : INoteService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly INoteRepository _noteRepository;
    private readonly ITagRepository _tagRepository;

    public NoteService(
        INoteRepository noteRepository,
        ICategoryRepository categoryRepository,
        ITagRepository tagRepository)
    {
        _noteRepository = noteRepository;
        _categoryRepository = categoryRepository;
        _tagRepository = tagRepository;
    }

    public Task<PagedResult<Note>> GetNotesAsync(
        Guid userId,
        NoteFilter filter,
        PagedQuery pagedQuery,
        SortRequest sortRequest,
        CancellationToken cancellationToken = default)
    {
        return _noteRepository.GetByUserAsync(userId, filter, pagedQuery, sortRequest, cancellationToken);
    }

    public async Task<Note> GetByIdAsync(Guid userId, Guid noteId, CancellationToken cancellationToken = default)
    {
        return await GetRequiredNoteAsync(userId, noteId, cancellationToken);
    }

    public async Task<Note> CreateAsync(
        Guid userId,
        string title,
        string content,
        Guid? categoryId,
        IReadOnlyCollection<Guid> tagIds,
        bool isFavorite,
        bool isArchived,
        bool isPinned,
        CancellationToken cancellationToken = default)
    {
        var normalizedTitle = ValidateRequiredTitle(title);
        var normalizedCategoryId = NormalizeOptionalGuid(categoryId);
        var normalizedTagIds = NormalizeTagIds(tagIds);

        await EnsureCategoryBelongsToUserAsync(userId, normalizedCategoryId, cancellationToken);
        await EnsureTagsBelongToUserAsync(userId, normalizedTagIds, cancellationToken);

        var note = new Note(normalizedTitle, content, userId, normalizedCategoryId);
        note.SetFavorite(isFavorite);
        note.SetArchived(isArchived);
        note.SetPinned(isPinned);
        note.ReplaceTags(normalizedTagIds);

        await _noteRepository.AddAsync(note, cancellationToken);
        return note;
    }

    public async Task<Note> UpdateAsync(
        Guid userId,
        Guid noteId,
        string title,
        string content,
        Guid? categoryId,
        IReadOnlyCollection<Guid> tagIds,
        bool isFavorite,
        bool isArchived,
        bool isPinned,
        CancellationToken cancellationToken = default)
    {
        var note = await GetRequiredNoteAsync(userId, noteId, cancellationToken);
        var normalizedTitle = ValidateRequiredTitle(title);
        var normalizedCategoryId = NormalizeOptionalGuid(categoryId);
        var normalizedTagIds = NormalizeTagIds(tagIds);

        await EnsureCategoryBelongsToUserAsync(userId, normalizedCategoryId, cancellationToken);
        await EnsureTagsBelongToUserAsync(userId, normalizedTagIds, cancellationToken);

        note.UpdateTitle(normalizedTitle);
        note.UpdateContent(content);
        note.SetCategory(normalizedCategoryId);
        note.SetFavorite(isFavorite);
        note.SetArchived(isArchived);
        note.SetPinned(isPinned);
        note.ReplaceTags(normalizedTagIds);

        await _noteRepository.UpdateAsync(note, cancellationToken);
        return note;
    }

    public async Task DeleteAsync(Guid userId, Guid noteId, CancellationToken cancellationToken = default)
    {
        var note = await GetRequiredNoteAsync(userId, noteId, cancellationToken);
        await _noteRepository.RemoveAsync(note, cancellationToken);
    }

    private async Task<Note> GetRequiredNoteAsync(Guid userId, Guid noteId, CancellationToken cancellationToken)
    {
        var note = await _noteRepository.GetByIdAsync(noteId, userId, cancellationToken);
        if (note is null)
        {
            throw new NotFoundException("Note was not found.");
        }

        return note;
    }

    private async Task EnsureCategoryBelongsToUserAsync(Guid userId, Guid? categoryId, CancellationToken cancellationToken)
    {
        if (!categoryId.HasValue)
        {
            return;
        }

        var category = await _categoryRepository.GetByIdAsync(categoryId.Value, userId, cancellationToken);
        if (category is null)
        {
            throw new ValidationException("Category must belong to the current user.");
        }
    }

    private async Task EnsureTagsBelongToUserAsync(Guid userId, IReadOnlyCollection<Guid> tagIds, CancellationToken cancellationToken)
    {
        if (tagIds.Count == 0)
        {
            return;
        }

        var tags = await _tagRepository.GetByIdsAsync(userId, tagIds, cancellationToken);
        if (tags.Count != tagIds.Count)
        {
            throw new ValidationException("All tags must belong to the current user.");
        }
    }

    private static string ValidateRequiredTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ValidationException("Note title is required.");
        }

        return title.Trim();
    }

    private static IReadOnlyCollection<Guid> NormalizeTagIds(IReadOnlyCollection<Guid> tagIds)
    {
        if (tagIds is null)
        {
            throw new ValidationException("Tag IDs are required.");
        }

        if (tagIds.Any(tagId => tagId == Guid.Empty))
        {
            throw new ValidationException("Tag IDs cannot contain empty values.");
        }

        return tagIds
            .Distinct()
            .ToArray();
    }

    private static Guid? NormalizeOptionalGuid(Guid? value)
    {
        return value == Guid.Empty ? null : value;
    }
}
