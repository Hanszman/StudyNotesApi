using FluentAssertions;
using StudyNotesApi.Application.Models.Categories;

namespace StudyNotesApi.UnitTests.Application.Models.Categories;

public class CategoryFilterTests
{
    [Fact]
    public void Constructor_should_trim_search_text()
    {
        var filter = new CategoryFilter("  backend  ");

        filter.Search.Should().Be("backend");
    }

    [Fact]
    public void Constructor_should_convert_blank_search_to_null()
    {
        var filter = new CategoryFilter("   ");

        filter.Search.Should().BeNull();
    }
}
