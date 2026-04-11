using FluentAssertions;
using StudyNotesApi.Application.Models.Notes;

namespace StudyNotesApi.UnitTests.Application.Models.Notes;

public class NoteFilterTests
{
    [Fact]
    public void Constructor_should_normalize_search_and_empty_guids()
    {
        var filter = new NoteFilter(
            search: "  ef core  ",
            categoryId: Guid.Empty,
            tagId: Guid.Empty,
            isFavorite: true,
            isArchived: false,
            isPinned: true);

        filter.Search.Should().Be("ef core");
        filter.CategoryId.Should().BeNull();
        filter.TagId.Should().BeNull();
        filter.IsFavorite.Should().BeTrue();
        filter.IsArchived.Should().BeFalse();
        filter.IsPinned.Should().BeTrue();
    }

    [Fact]
    public void Constructor_should_keep_valid_guids_and_convert_blank_search_to_null()
    {
        var categoryId = Guid.NewGuid();
        var tagId = Guid.NewGuid();

        var filter = new NoteFilter(search: "   ", categoryId: categoryId, tagId: tagId);

        filter.Search.Should().BeNull();
        filter.CategoryId.Should().Be(categoryId);
        filter.TagId.Should().Be(tagId);
    }
}
