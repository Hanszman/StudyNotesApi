using FluentAssertions;
using StudyNotesApi.Application.Common.Exceptions;
using StudyNotesApi.Application.Models.Common;

namespace StudyNotesApi.UnitTests.Application.Models.Common;

public class PagedQueryTests
{
    [Fact]
    public void Constructor_should_store_valid_values_and_calculate_skip()
    {
        var query = new PagedQuery(pageNumber: 3, pageSize: 20);

        query.PageNumber.Should().Be(3);
        query.PageSize.Should().Be(20);
        query.Skip.Should().Be(40);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    [InlineData(1, 101)]
    public void Constructor_should_throw_validation_exception_when_values_are_invalid(int pageNumber, int pageSize)
    {
        var action = () => new PagedQuery(pageNumber, pageSize);

        action.Should().Throw<ValidationException>();
    }
}
