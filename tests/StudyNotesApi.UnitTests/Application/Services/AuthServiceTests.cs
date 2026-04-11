using FluentAssertions;
using Moq;
using StudyNotesApi.Application.Common.Exceptions;
using StudyNotesApi.Application.Interfaces.Repositories;
using StudyNotesApi.Application.Interfaces.Security;
using StudyNotesApi.Application.Models.Auth;
using StudyNotesApi.Application.Services;
using StudyNotesApi.Domain.Entities;

namespace StudyNotesApi.UnitTests.Application.Services;

public class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_should_create_user_when_email_is_available()
    {
        var userRepository = new Mock<IUserRepository>();
        var passwordHasher = new Mock<IPasswordHasher>();
        var jwtTokenGenerator = new Mock<IJwtTokenGenerator>();
        var service = new AuthService(userRepository.Object, passwordHasher.Object, jwtTokenGenerator.Object);

        passwordHasher.Setup(hasher => hasher.Hash("123456")).Returns("hashed-password");
        userRepository.Setup(repository => repository.EmailExistsAsync("victor@email.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await service.RegisterAsync(new RegisterUserRequest("Victor", " Victor@Email.com ", "123456"));

        result.Name.Should().Be("Victor");
        result.Email.Should().Be("victor@email.com");
        result.PasswordHash.Should().Be("hashed-password");
        userRepository.Verify(repository => repository.AddAsync(It.Is<User>(user =>
            user.Name == "Victor" &&
            user.Email == "victor@email.com" &&
            user.PasswordHash == "hashed-password"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_should_throw_conflict_when_email_already_exists()
    {
        var userRepository = new Mock<IUserRepository>();
        var service = new AuthService(userRepository.Object, Mock.Of<IPasswordHasher>(), Mock.Of<IJwtTokenGenerator>());

        userRepository.Setup(repository => repository.EmailExistsAsync("victor@email.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var action = async () => await service.RegisterAsync(new RegisterUserRequest("Victor", "victor@email.com", "123456"));

        await action.Should().ThrowAsync<ConflictException>();
    }

    [Theory]
    [InlineData("", "victor@email.com", "123456")]
    [InlineData("Victor", "", "123456")]
    [InlineData("Victor", "victor@email.com", "")]
    public async Task RegisterAsync_should_throw_validation_exception_when_required_fields_are_missing(string name, string email, string password)
    {
        var service = new AuthService(Mock.Of<IUserRepository>(), Mock.Of<IPasswordHasher>(), Mock.Of<IJwtTokenGenerator>());

        var action = async () => await service.RegisterAsync(new RegisterUserRequest(name, email, password));

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task LoginAsync_should_return_token_for_valid_credentials()
    {
        var userRepository = new Mock<IUserRepository>();
        var passwordHasher = new Mock<IPasswordHasher>();
        var jwtTokenGenerator = new Mock<IJwtTokenGenerator>();
        var service = new AuthService(userRepository.Object, passwordHasher.Object, jwtTokenGenerator.Object);
        var user = new User("Victor", "victor@email.com", "hashed-password");
        var expectedToken = new AuthTokenResult("jwt-token", 3600);

        userRepository.Setup(repository => repository.GetByEmailAsync("victor@email.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        passwordHasher.Setup(hasher => hasher.Verify("123456", "hashed-password")).Returns(true);
        jwtTokenGenerator.Setup(generator => generator.GenerateToken(user)).Returns(expectedToken);

        var result = await service.LoginAsync(new LoginRequest(" Victor@Email.com ", "123456"));

        result.Should().Be(expectedToken);
    }

    [Fact]
    public async Task LoginAsync_should_throw_unauthorized_when_user_does_not_exist()
    {
        var userRepository = new Mock<IUserRepository>();
        var service = new AuthService(userRepository.Object, Mock.Of<IPasswordHasher>(), Mock.Of<IJwtTokenGenerator>());

        userRepository.Setup(repository => repository.GetByEmailAsync("victor@email.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var action = async () => await service.LoginAsync(new LoginRequest("victor@email.com", "123456"));

        await action.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task LoginAsync_should_throw_unauthorized_when_password_is_invalid()
    {
        var userRepository = new Mock<IUserRepository>();
        var passwordHasher = new Mock<IPasswordHasher>();
        var service = new AuthService(userRepository.Object, passwordHasher.Object, Mock.Of<IJwtTokenGenerator>());
        var user = new User("Victor", "victor@email.com", "hashed-password");

        userRepository.Setup(repository => repository.GetByEmailAsync("victor@email.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        passwordHasher.Setup(hasher => hasher.Verify("123456", "hashed-password")).Returns(false);

        var action = async () => await service.LoginAsync(new LoginRequest("victor@email.com", "123456"));

        await action.Should().ThrowAsync<UnauthorizedException>();
    }

    [Theory]
    [InlineData("", "123456")]
    [InlineData("victor@email.com", "")]
    public async Task LoginAsync_should_throw_validation_exception_when_required_fields_are_missing(string email, string password)
    {
        var service = new AuthService(Mock.Of<IUserRepository>(), Mock.Of<IPasswordHasher>(), Mock.Of<IJwtTokenGenerator>());

        var action = async () => await service.LoginAsync(new LoginRequest(email, password));

        await action.Should().ThrowAsync<ValidationException>();
    }
}
