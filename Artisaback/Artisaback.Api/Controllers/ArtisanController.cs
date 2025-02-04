using Artisaback.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Artisaback.Api.Controllers;

[ApiController]
[Route("api/artisans")]
public class ArtisanController : ControllerBase
{
    private readonly IAuthService _authService;

    public ArtisanController(IAuthService authService)
    {
        _authService = authService;
    }

    [Authorize(Roles = "Artisan")]
    [HttpGet("{id}/products")]
    public IActionResult GetArtisanProducts(int id)
    {
        return Ok($"Artisan {id}'s products");
    }
}