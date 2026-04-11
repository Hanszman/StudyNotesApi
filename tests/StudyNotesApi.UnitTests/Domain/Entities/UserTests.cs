using FluentAssertions;
using StudyNotesApi.Domain.Entities;

namespace StudyNotesApi.UnitTests.Domain.Entities;

public class UserTests
{
    [Fact]
    public void Constructor_should_normalize_required_fields()
    {
        var user = new User("  Victor  ", "  Victor@Email.com  ", "  hash-value  ");

        user.Name.Should().Be("Victor");
        user.Email.Should().Be("victor@email.com");
        user.PasswordHash.Should().Be("hash-value");
        user.Id.Should().NotBe(Guid.Empty);
        user.CreatedAt.Should().NotBe(default);
    }

    [Fact]
    public void Update_methods_should_change_values_and_mark_entity_as_updated()
    {
        var user = new User("Victor", "victor@email.com", "hash-value");

        user.UpdateName("  Victor Hugo  ");
        user.UpdateEmail("  NewEmail@Email.com  ");
        user.UpdatePasswordHash("  new-hash  ");

        user.Name.Should().Be("Victor Hugo");
        user.Email.Should().Be("newemail@email.com");
        user.PasswordHash.Should().Be("new-hash");
        user.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_should_throw_when_required_fields_are_empty()
    {
        var action = () => new User(" ", " ", " ");

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateName_should_throw_when_name_is_invalid()
    {
        var user = new User("Victor", "victor@email.com", "hash-value");

        var action = () => user.UpdateName(" ");

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateEmail_should_throw_when_email_is_invalid()
    {
        var user = new User("Victor", "victor@email.com", "hash-value");

        var action = () => user.UpdateEmail(" ");

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdatePasswordHash_should_throw_when_hash_is_invalid()
    {
        var user = new User("Victor", "victor@email.com", "hash-value");

        var action = () => user.UpdatePasswordHash(" ");

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Private_constructor_should_exist_for_ef_core()
    {
        var user = Activator.CreateInstance(typeof(User), nonPublic: true);

        user.Should().NotBeNull();
    }

    [Fact]
    public void Navigation_properties_should_expose_empty_read_only_collections_by_default()
    {
        var user = new User("Victor", "victor@email.com", "hash-value");

        user.Categories.Should().BeEmpty();
        user.Notes.Should().BeEmpty();
        user.Tags.Should().BeEmpty();
    }
}
