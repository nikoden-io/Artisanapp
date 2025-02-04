using Artisaback.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Artisaback.Api.Controllers;

[ApiController]
[Route("api/artisan")]
public class CustomerController : ControllerBase
{
    private readonly IAuthService _authService;

    public CustomerController(IAuthService authService)
    {
        _authService = authService;
    }
}