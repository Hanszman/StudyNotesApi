using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Microsoft.Extensions.Options;
using StudyNotesApi.Application.Common.Exceptions;
using StudyNotesApi.Api.Security.Jwt;
using StudyNotesApi.Domain.Entities;

namespace StudyNotesApi.UnitTests.Infrastructure.Security.Jwt;

public class JwtTokenGeneratorTests
{
    [Fact]
    public void GenerateToken_should_create_a_bearer_token_with_expected_claims()
    {
        var options = Options.Create(new JwtOptions
        {
            Secret = "study-notes-api-unit-test-secret-key",
            Issuer = "StudyNotesApi",
            Audience = "StudyNotesApiUsers",
            ExpirationMinutes = 60
        });

        var generator = new JwtTokenGenerator(options);
        var user = new User("Victor", "victor@email.com", "hash");

        var result = generator.GenerateToken(user);

        result.TokenType.Should().Be("Bearer");
        result.ExpiresIn.Should().Be(3600);
        result.AccessToken.Should().NotBeNullOrWhiteSpace();

        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);
        token.Issuer.Should().Be("StudyNotesApi");
        token.Audiences.Should().Contain("StudyNotesApiUsers");
        token.Claims.Should().Contain(claim => claim.Type == JwtRegisteredClaimNames.Sub && claim.Value == user.Id.ToString());
        token.Claims.Should().Contain(claim => claim.Type == JwtRegisteredClaimNames.Email && claim.Value == user.Email);
        token.Claims.Should().Contain(claim => claim.Type == JwtRegisteredClaimNames.UniqueName && claim.Value == user.Name);
        token.Claims.Should().Contain(claim => claim.Type == JwtRegisteredClaimNames.Jti);
    }

    [Fact]
    public void GenerateToken_should_throw_when_secret_is_too_short()
    {
        var options = Options.Create(new JwtOptions
        {
            Secret = "short-secret",
            Issuer = "StudyNotesApi",
            Audience = "StudyNotesApiUsers",
            ExpirationMinutes = 60
        });

        var generator = new JwtTokenGenerator(options);
        var user = new User("Victor", "victor@email.com", "hash");

        var action = () => generator.GenerateToken(user);

        action.Should().Throw<ValidationException>();
    }
}
