using FluentAssertions;
using StudyNotesApi.Application.Common.Exceptions;
using StudyNotesApi.Application.Models.Common;

namespace StudyNotesApi.UnitTests.Application.Models.Common;

public class PagedResultTests
{
    [Fact]
    public void Constructor_should_store_values_and_calculate_paging_metadata()
    {
        var result = new PagedResult<string>(["a", "b"], pageNumber: 2, pageSize: 10, totalCount: 25);

        result.Items.Should().BeEquivalentTo(["a", "b"]);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(25);
        result.TotalPages.Should().Be(3);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void Constructor_should_report_zero_pages_when_total_count_is_zero()
    {
        var result = new PagedResult<string>([], pageNumber: 1, pageSize: 10, totalCount: 0);

        result.TotalPages.Should().Be(0);
        result.HasPreviousPage.Should().BeFalse();
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void Constructor_should_not_have_next_page_on_the_last_page()
    {
        var result = new PagedResult<string>(["a"], pageNumber: 3, pageSize: 10, totalCount: 21);

        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void Constructor_should_throw_when_items_are_null()
    {
        var action = () => new PagedResult<string>(null!, pageNumber: 1, pageSize: 10, totalCount: 0);

        action.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(1, 0, 0)]
    [InlineData(1, 10, -1)]
    public void Constructor_should_throw_validation_exception_when_numeric_values_are_invalid(int pageNumber, int pageSize, int totalCount)
    {
        var action = () => new PagedResult<string>([], pageNumber, pageSize, totalCount);

        action.Should().Throw<ValidationException>();
    }
}
