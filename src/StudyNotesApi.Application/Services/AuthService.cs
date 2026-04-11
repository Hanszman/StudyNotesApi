using StudyNotesApi.Application.Common.Exceptions;
using StudyNotesApi.Application.Interfaces.Repositories;
using StudyNotesApi.Application.Interfaces.Security;
using StudyNotesApi.Application.Interfaces.Services;
using StudyNotesApi.Application.Models.Auth;
using StudyNotesApi.Domain.Entities;

namespace StudyNotesApi.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<User> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRegisterRequest(request);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        if (await _userRepository.EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            throw new ConflictException("A user with this email already exists.");
        }

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = new User(request.Name, normalizedEmail, passwordHash);

        await _userRepository.AddAsync(user, cancellationToken);
        return user;
    }

    public async Task<AuthTokenResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        ValidateLoginRequest(request);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        return _jwtTokenGenerator.GenerateToken(user);
    }

    private static void ValidateRegisterRequest(RegisterUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException("Name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ValidationException("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationException("Password is required.");
        }
    }

    private static void ValidateLoginRequest(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ValidationException("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationException("Password is required.");
        }
    }
}
