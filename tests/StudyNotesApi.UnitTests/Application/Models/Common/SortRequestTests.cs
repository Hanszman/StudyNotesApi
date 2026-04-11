using FluentAssertions;
using StudyNotesApi.Application.Models.Common;

namespace StudyNotesApi.UnitTests.Application.Models.Common;

public class SortRequestTests
{
    [Fact]
    public void Constructor_should_trim_sort_by_and_keep_direction()
    {
        var request = new SortRequest("  createdAt  ", SortDirection.Asc);

        request.SortBy.Should().Be("createdAt");
        request.Direction.Should().Be(SortDirection.Asc);
        request.HasSorting.Should().BeTrue();
    }

    [Fact]
    public void Constructor_should_convert_blank_sort_by_to_null()
    {
        var request = new SortRequest("   ");

        request.SortBy.Should().BeNull();
        request.Direction.Should().Be(SortDirection.Desc);
        request.HasSorting.Should().BeFalse();
    }
}
