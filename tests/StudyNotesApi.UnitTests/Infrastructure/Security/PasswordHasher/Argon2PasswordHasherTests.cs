using FluentAssertions;
using StudyNotesApi.Application.Common.Exceptions;
using StudyNotesApi.Api.Security.PasswordHasher;

namespace StudyNotesApi.UnitTests.Infrastructure.Security.PasswordHasher;

public class Argon2PasswordHasherTests
{
    [Fact]
    public void Hash_should_generate_an_argon2id_hash_that_can_be_verified()
    {
        var hasher = new Argon2PasswordHasher();

        var hash = hasher.Hash("super-secret-password");

        hash.Should().StartWith("argon2id$");
        hasher.Verify("super-secret-password", hash).Should().BeTrue();
        hasher.Verify("wrong-password", hash).Should().BeFalse();
    }

    [Fact]
    public void Hash_should_generate_different_hashes_for_the_same_password()
    {
        var hasher = new Argon2PasswordHasher();

        var hashA = hasher.Hash("super-secret-password");
        var hashB = hasher.Hash("super-secret-password");

        hashA.Should().NotBe(hashB);
    }

    [Fact]
    public void Verify_should_return_false_for_invalid_hash_format()
    {
        var hasher = new Argon2PasswordHasher();

        hasher.Verify("super-secret-password", "invalid-hash").Should().BeFalse();
    }

    [Theory]
    [InlineData("argon2id$m=65536,t=4$YWJjZA==$ZWZnaA==")]
    [InlineData("argon2id$m=x,t=4,p=4$YWJjZA==$ZWZnaA==")]
    [InlineData("argon2id$m=65536,t=4,p=4$invalid-base64$ZWZnaA==")]
    [InlineData("argon2id$m=65536,t=4,p=4$YWJjZA==$invalid-base64")]
    [InlineData("argon2id$m=65536,t=4,x=4$YWJjZA==$ZWZnaA==")]
    public void Verify_should_return_false_for_invalid_hash_payload(string passwordHash)
    {
        var hasher = new Argon2PasswordHasher();

        hasher.Verify("super-secret-password", passwordHash).Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Hash_should_throw_when_password_is_empty(string password)
    {
        var hasher = new Argon2PasswordHasher();
        var action = () => hasher.Hash(password);

        action.Should().Throw<ValidationException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Verify_should_throw_when_password_or_hash_are_empty(string value)
    {
        var hasher = new Argon2PasswordHasher();

        var passwordAction = () => hasher.Verify(value, "argon2id$m=1,t=1,p=1$YWJjZA==$ZWZnaA==");
        var hashAction = () => hasher.Verify("password", value);

        passwordAction.Should().Throw<ValidationException>();
        hashAction.Should().Throw<ValidationException>();
    }
}
