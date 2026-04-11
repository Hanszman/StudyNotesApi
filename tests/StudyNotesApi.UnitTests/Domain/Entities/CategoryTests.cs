using FluentAssertions;
using StudyNotesApi.Domain.Entities;

namespace StudyNotesApi.UnitTests.Domain.Entities;

public class CategoryTests
{
    [Fact]
    public void Constructor_should_normalize_name_and_optional_color()
    {
        var userId = Guid.NewGuid();

        var category = new Category("  Backend  ", userId, "  #00FFAA  ");

        category.Name.Should().Be("Backend");
        category.Color.Should().Be("#00FFAA");
        category.UserId.Should().Be(userId);
    }

    [Fact]
    public void UpdateColor_should_convert_whitespace_to_null()
    {
        var category = new Category("Backend", Guid.NewGuid(), "#FFFFFF");

        category.UpdateColor("   ");

        category.Color.Should().BeNull();
        category.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_should_throw_when_user_id_is_empty()
    {
        var action = () => new Category("Backend", Guid.Empty);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Rename_should_update_the_name_and_timestamp()
    {
        var category = new Category("Backend", Guid.NewGuid(), null);

        category.Rename("  Architecture  ");

        category.Name.Should().Be("Architecture");
        category.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_should_throw_when_name_is_invalid()
    {
        var action = () => new Category(" ", Guid.NewGuid());

        action.Should().Throw<ArgumentException>();
    }
}
