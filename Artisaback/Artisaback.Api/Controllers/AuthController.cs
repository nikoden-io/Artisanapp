using System.ComponentModel.DataAnnotations;
using Artisaback.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Artisaback.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var token = await _authService.RegisterAsync(request.Email, request.Password, request.Role);
            return Created(string.Empty, new { Token = token });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var token = await _authService.LoginAsync(request.Email, request.Password);
        return token != null ? Ok(new { Token = token }) : Unauthorized("Invalid credentials");
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshRequest request)
    {
        var newToken = await _authService.RefreshTokenAsync(request.RefreshToken);
        return newToken != null ? Ok(new { Token = newToken }) : Unauthorized("Invalid refresh token");
    }
}

public record RegisterRequest(
    [Required] [EmailAddress] string Email,
    [Required] [MinLength(6)] string Password,
    [Required] string Role);

public record LoginRequest([Required] [EmailAddress] string Email, [Required] string Password);

public record RefreshRequest([Required] string RefreshToken);