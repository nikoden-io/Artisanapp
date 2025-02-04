using Artisaback.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Artisaback.Api.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IAuthService _authService;

    public AdminController(IAuthService authService)
    {
        _authService = authService;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("users")]
    public IActionResult GetAllUsers()
    {
        return Ok("Admin users management");
    }
}