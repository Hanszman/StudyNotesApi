using FluentAssertions;
using StudyNotesApi.Domain.Entities;

namespace StudyNotesApi.UnitTests.Domain.Entities;

public class NoteTagTests
{
    [Fact]
    public void Constructor_should_store_note_and_tag_identifiers()
    {
        var noteId = Guid.NewGuid();
        var tagId = Guid.NewGuid();

        var noteTag = new NoteTag(noteId, tagId);

        noteTag.NoteId.Should().Be(noteId);
        noteTag.TagId.Should().Be(tagId);
    }

    [Fact]
    public void Constructor_should_throw_when_any_identifier_is_empty()
    {
        var action = () => new NoteTag(Guid.Empty, Guid.NewGuid());

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_should_throw_when_tag_id_is_empty()
    {
        var action = () => new NoteTag(Guid.NewGuid(), Guid.Empty);

        action.Should().Throw<ArgumentException>();
    }
}
