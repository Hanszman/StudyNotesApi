using FluentAssertions;
using StudyNotesApi.Application.Models.Tags;

namespace StudyNotesApi.UnitTests.Application.Models.Tags;

public class TagFilterTests
{
    [Fact]
    public void Constructor_should_trim_search_text()
    {
        var filter = new TagFilter("  dotnet  ");

        filter.Search.Should().Be("dotnet");
    }

    [Fact]
    public void Constructor_should_convert_blank_search_to_null()
    {
        var filter = new TagFilter("   ");

        filter.Search.Should().BeNull();
    }
}
