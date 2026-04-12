using StudyNotesApi.Application.Common.Exceptions;
using StudyNotesApi.Application.Interfaces.Repositories;
using StudyNotesApi.Application.Interfaces.Services;
using StudyNotesApi.Application.Models.Common;
using StudyNotesApi.Application.Models.Tags;
using StudyNotesApi.Domain.Entities;

namespace StudyNotesApi.Application.Services;

public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;

    public TagService(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public Task<PagedResult<Tag>> GetTagsAsync(
        Guid userId,
        TagFilter filter,
        PagedQuery pagedQuery,
        SortRequest sortRequest,
        CancellationToken cancellationToken = default)
    {
        return _tagRepository.GetByUserAsync(userId, filter, pagedQuery, sortRequest, cancellationToken);
    }

    public async Task<Tag> GetByIdAsync(Guid userId, Guid tagId, CancellationToken cancellationToken = default)
    {
        return await GetRequiredTagAsync(userId, tagId, cancellationToken);
    }

    public async Task<Tag> CreateAsync(Guid userId, string name, CancellationToken cancellationToken = default)
    {
        var normalizedName = ValidateRequiredName(name);

        if (await _tagRepository.ExistsByNameAsync(userId, normalizedName, null, cancellationToken))
        {
            throw new ConflictException("A tag with this name already exists.");
        }

        var tag = new Tag(normalizedName, userId);
        await _tagRepository.AddAsync(tag, cancellationToken);

        return tag;
    }

    public async Task<Tag> UpdateAsync(Guid userId, Guid tagId, string name, CancellationToken cancellationToken = default)
    {
        var tag = await GetRequiredTagAsync(userId, tagId, cancellationToken);
        var normalizedName = ValidateRequiredName(name);

        if (await _tagRepository.ExistsByNameAsync(userId, normalizedName, tagId, cancellationToken))
        {
            throw new ConflictException("A tag with this name already exists.");
        }

        tag.Rename(normalizedName);
        await _tagRepository.UpdateAsync(tag, cancellationToken);

        return tag;
    }

    public async Task DeleteAsync(Guid userId, Guid tagId, CancellationToken cancellationToken = default)
    {
        var tag = await GetRequiredTagAsync(userId, tagId, cancellationToken);
        await _tagRepository.RemoveAsync(tag, cancellationToken);
    }

    private async Task<Tag> GetRequiredTagAsync(Guid userId, Guid tagId, CancellationToken cancellationToken)
    {
        var tag = await _tagRepository.GetByIdAsync(tagId, userId, cancellationToken);
        if (tag is null)
        {
            throw new NotFoundException("Tag was not found.");
        }

        return tag;
    }

    private static string ValidateRequiredName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException("Tag name is required.");
        }

        return name.Trim();
    }
}
