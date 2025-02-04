using Artisaback.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Artisaback.Api.Controllers;

[ApiController]
[Route("api/delivery")]
public class DeliveryController : ControllerBase
{
    private readonly IAuthService _authService;

    public DeliveryController(IAuthService authService)
    {
        _authService = authService;
    }

    [Authorize(Roles = "DeliveryPartner")]
    [HttpGet("orders")]
    public IActionResult GetDeliveryOrders()
    {
        return Ok("Delivery orders");
    }
}