using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WedNest.Application.DTOs;
using WedNest.Application.Services;

namespace WedNest.API.Controllers;

[ApiController]
[Route("api/weddings/{weddingId:guid}/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _service;

    public OrdersController(OrderService service) => _service = service;

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll(Guid weddingId) => Ok(await _service.GetByWeddingAsync(weddingId));

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result != null ? Ok(result) : NotFound();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(Guid weddingId, [FromBody] CreateOrderRequest request)
    {
        request.WeddingId = weddingId;
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("email")?.Value ?? "");
        var result = await _service.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { weddingId, id = result.Id }, result);
    }

    [Authorize]
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        var result = await _service.UpdateStatusAsync(id, request.Status);
        return result != null ? Ok(result) : NotFound();
    }
}

public class UpdateStatusRequest
{
    public string Status { get; set; } = string.Empty;
}
