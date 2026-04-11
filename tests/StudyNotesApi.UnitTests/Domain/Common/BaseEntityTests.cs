using FluentAssertions;
using StudyNotesApi.Domain.Common;

namespace StudyNotesApi.UnitTests.Domain.Common;

public class BaseEntityTests
{
    [Fact]
    public void Parameterless_constructor_should_leave_default_values()
    {
        var entity = new TestEntity();

        entity.Id.Should().Be(Guid.Empty);
        entity.CreatedAt.Should().Be(default);
        entity.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Constructor_with_null_id_should_generate_identifier_and_created_timestamp()
    {
        var entity = new TestEntity(null);

        entity.Id.Should().NotBe(Guid.Empty);
        entity.CreatedAt.Should().NotBe(default);
    }

    [Fact]
    public void Constructor_with_explicit_id_should_keep_the_given_identifier()
    {
        var id = Guid.NewGuid();
        var entity = new TestEntity(id);

        entity.Id.Should().Be(id);
    }

    [Fact]
    public void MarkAsUpdated_should_fill_updated_at()
    {
        var entity = new TestEntity(Guid.NewGuid());

        entity.Touch();

        entity.UpdatedAt.Should().NotBeNull();
    }

    private sealed class TestEntity : BaseEntity
    {
        public TestEntity()
        {
        }

        public TestEntity(Guid? id)
            : base(id)
        {
        }

        public void Touch()
        {
            MarkAsUpdated();
        }
    }
}
