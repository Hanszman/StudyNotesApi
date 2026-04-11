using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StudyNotesApi.Api.Controllers;
using StudyNotesApi.Api.DTOs.Auth;
using StudyNotesApi.Application.Interfaces.Services;
using StudyNotesApi.Application.Models.Auth;
using StudyNotesApi.Domain.Entities;

namespace StudyNotesApi.UnitTests.Api.Controllers;

public class AuthControllerTests
{
    [Fact]
    public async Task Register_should_return_created_response_with_user_payload()
    {
        var authService = new Mock<IAuthService>();
        var controller = new AuthController(authService.Object);
        var user = new User("Victor", "victor@email.com", "hashed-password");

        authService.Setup(service => service.RegisterAsync(
                It.Is<RegisterUserRequest>(request =>
                    request.Name == "Victor" &&
                    request.Email == "victor@email.com" &&
                    request.Password == "123456"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await controller.Register(new RegisterRequestDto("Victor", "victor@email.com", "123456"), CancellationToken.None);

        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        objectResult.Value.Should().BeEquivalentTo(new RegisterResponseDto(user.Id, user.Name, user.Email));
    }

    [Fact]
    public async Task Login_should_return_ok_response_with_token_payload()
    {
        var authService = new Mock<IAuthService>();
        var controller = new AuthController(authService.Object);
        var token = new AuthTokenResult("jwt-token", 3600, "Bearer");

        authService.Setup(service => service.LoginAsync(
                It.Is<LoginRequest>(request =>
                    request.Email == "victor@email.com" &&
                    request.Password == "123456"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        var result = await controller.Login(new LoginRequestDto("victor@email.com", "123456"), CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new LoginResponseDto(token.AccessToken, token.ExpiresIn, token.TokenType));
    }
}
