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

    [Fact]
    public void Rename_should_throw_when_name_is_invalid()
    {
        var tag = new Tag("dotnet", Guid.NewGuid());

        var action = () => tag.Rename(" ");

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Private_constructor_should_exist_for_ef_core()
    {
        var tag = Activator.CreateInstance(typeof(Tag), nonPublic: true);

        tag.Should().NotBeNull();
    }

    [Fact]
    public void Navigation_properties_should_start_empty_or_null()
    {
        var tag = new Tag("dotnet", Guid.NewGuid());

        tag.User.Should().BeNull();
        tag.NoteTags.Should().BeEmpty();
    }
}
