using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyNotesApi.Api.DTOs.Auth;
using StudyNotesApi.Application.Interfaces.Services;
using StudyNotesApi.Application.Models.Auth;

namespace StudyNotesApi.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegisterResponseDto>> Register(
        [FromBody] RegisterRequestDto request,
        CancellationToken cancellationToken)
    {
        var user = await _authService.RegisterAsync(
            new RegisterUserRequest(request.Name, request.Email, request.Password),
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            new RegisterResponseDto(user.Id, user.Name, user.Email));
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponseDto>> Login(
        [FromBody] LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        var token = await _authService.LoginAsync(
            new LoginRequest(request.Email, request.Password),
            cancellationToken);

        return Ok(new LoginResponseDto(token.AccessToken, token.ExpiresIn, token.TokenType));
    }
}
