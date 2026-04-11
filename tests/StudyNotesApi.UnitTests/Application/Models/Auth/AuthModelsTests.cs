using FluentAssertions;
using StudyNotesApi.Application.Models.Auth;

namespace StudyNotesApi.UnitTests.Application.Models.Auth;

public class AuthModelsTests
{
    [Fact]
    public void RegisterUserRequest_should_store_constructor_values()
    {
        var request = new RegisterUserRequest("Victor", "victor@email.com", "123456");

        request.Name.Should().Be("Victor");
        request.Email.Should().Be("victor@email.com");
        request.Password.Should().Be("123456");
    }

    [Fact]
    public void LoginRequest_should_store_constructor_values()
    {
        var request = new LoginRequest("victor@email.com", "123456");

        request.Email.Should().Be("victor@email.com");
        request.Password.Should().Be("123456");
    }

    [Fact]
    public void AuthTokenResult_should_store_constructor_values()
    {
        var result = new AuthTokenResult("jwt-token", 3600);

        result.AccessToken.Should().Be("jwt-token");
        result.ExpiresIn.Should().Be(3600);
        result.TokenType.Should().Be("Bearer");
    }
}
