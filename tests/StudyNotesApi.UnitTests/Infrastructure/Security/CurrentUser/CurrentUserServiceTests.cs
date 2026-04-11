using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using StudyNotesApi.Application.Common.Exceptions;
using StudyNotesApi.Api.Security.CurrentUser;

namespace StudyNotesApi.UnitTests.Infrastructure.Security.CurrentUser;

public class CurrentUserServiceTests
{
    [Fact]
    public void IsAuthenticated_should_reflect_the_http_context_identity()
    {
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };

        httpContextAccessor.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString())], "Bearer"));

        var service = new CurrentUserService(httpContextAccessor);

        service.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void GetRequiredUserId_should_return_the_authenticated_user_identifier()
    {
        var userId = Guid.NewGuid();
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };

        httpContextAccessor.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, userId.ToString())], "Bearer"));

        var service = new CurrentUserService(httpContextAccessor);

        service.GetRequiredUserId().Should().Be(userId);
    }

    [Fact]
    public void GetRequiredUserId_should_throw_when_request_is_not_authenticated()
    {
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };

        var service = new CurrentUserService(httpContextAccessor);
        var action = () => service.GetRequiredUserId();

        action.Should().Throw<UnauthorizedException>();
    }

    [Fact]
    public void GetRequiredUserId_should_throw_when_claim_is_invalid()
    {
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };

        httpContextAccessor.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, "invalid-guid")], "Bearer"));

        var service = new CurrentUserService(httpContextAccessor);
        var action = () => service.GetRequiredUserId();

        action.Should().Throw<UnauthorizedException>();
    }
}
