using FluentAssertions;
using StudyNotesApi.Domain.Entities;

namespace StudyNotesApi.UnitTests.Domain.Entities;

public class TagTests
{
    [Fact]
    public void Constructor_should_normalize_the_name()
    {
        var tag = new Tag("  dotnet  ", Guid.NewGuid());

        tag.Name.Should().Be("dotnet");
    }

    [Fact]
    public void Rename_should_update_the_name_and_timestamp()
    {
        var tag = new Tag("dotnet", Guid.NewGuid());

        tag.Rename("  csharp  ");

        tag.Name.Should().Be("csharp");
        tag.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_should_throw_when_name_is_empty()
    {
        var action = () => new Tag(" ", Guid.NewGuid());

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_should_throw_when_user_id_is_empty()
    {
        var action = () => new Tag("dotnet", Guid.Empty);

        action.Should().Throw<ArgumentException>();
    }
}
