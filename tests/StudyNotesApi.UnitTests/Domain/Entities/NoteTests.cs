using FluentAssertions;
using StudyNotesApi.Domain.Entities;

namespace StudyNotesApi.UnitTests.Domain.Entities;

public class NoteTests
{
    [Fact]
    public void Constructor_should_require_a_title_and_normalize_content()
    {
        var note = new Note("  Study EF Core  ", "  relationship mapping  ", Guid.NewGuid());

        note.Title.Should().Be("Study EF Core");
        note.Content.Should().Be("relationship mapping");
        note.IsFavorite.Should().BeFalse();
        note.IsArchived.Should().BeFalse();
        note.IsPinned.Should().BeFalse();
    }

    [Fact]
    public void Mutation_methods_should_update_the_note_state()
    {
        var categoryId = Guid.NewGuid();
        var note = new Note("Title", "Content", Guid.NewGuid());

        note.UpdateTitle("  Updated title  ");
        note.UpdateContent("  Updated content  ");
        note.SetCategory(categoryId);
        note.SetFavorite(true);
        note.SetArchived(true);
        note.SetPinned(true);

        note.Title.Should().Be("Updated title");
        note.Content.Should().Be("Updated content");
        note.CategoryId.Should().Be(categoryId);
        note.IsFavorite.Should().BeTrue();
        note.IsArchived.Should().BeTrue();
        note.IsPinned.Should().BeTrue();
        note.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void SetCategory_should_treat_empty_guid_as_null()
    {
        var note = new Note("Title", "Content", Guid.NewGuid(), Guid.Empty);

        note.CategoryId.Should().BeNull();
    }

    [Fact]
    public void Constructor_should_throw_when_title_is_empty()
    {
        var action = () => new Note(" ", "Content", Guid.NewGuid());

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_should_throw_when_user_id_is_empty()
    {
        var action = () => new Note("Title", "Content", Guid.Empty);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateContent_should_convert_whitespace_to_empty_string()
    {
        var note = new Note("Title", "Content", Guid.NewGuid());

        note.UpdateContent("   ");

        note.Content.Should().BeEmpty();
    }

    [Fact]
    public void SetCategory_should_allow_null_category()
    {
        var note = new Note("Title", "Content", Guid.NewGuid(), Guid.NewGuid());

        note.SetCategory(null);

        note.CategoryId.Should().BeNull();
    }

    [Fact]
    public void UpdateTitle_should_throw_when_title_is_invalid()
    {
        var note = new Note("Title", "Content", Guid.NewGuid());

        var action = () => note.UpdateTitle(" ");

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Private_constructor_should_exist_for_ef_core()
    {
        var note = Activator.CreateInstance(typeof(Note), nonPublic: true);

        note.Should().NotBeNull();
    }

    [Fact]
    public void Navigation_properties_should_start_empty_or_null()
    {
        var note = new Note("Title", "Content", Guid.NewGuid());

        note.User.Should().BeNull();
        note.Category.Should().BeNull();
        note.NoteTags.Should().BeEmpty();
    }
}
