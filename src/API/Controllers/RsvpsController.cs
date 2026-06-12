using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WedNest.Application.DTOs;
using WedNest.Application.Services;

namespace WedNest.API.Controllers;

[Authorize]
[ApiController]
[Route("api/weddings/{weddingId:guid}/rsvps")]
public class RsvpsController : ControllerBase
{
    private readonly RsvpService _service;

    public RsvpsController(RsvpService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<GuestRsvpDto>>> GetAll(Guid weddingId)
        => Ok(await _service.GetAllAsync(weddingId));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GuestRsvpDto>> GetById(Guid weddingId, Guid id)
    {
        var result = await _service.GetByIdAsync(weddingId, id);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<GuestRsvpDto>> Create(Guid weddingId, [FromBody] CreateRsvpRequest request)
    {
        var result = await _service.CreateAsync(weddingId, request);
        return CreatedAtAction(nameof(GetById), new { weddingId, id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<GuestRsvpDto>> Update(Guid weddingId, Guid id, [FromBody] UpdateRsvpRequest request)
    {
        var result = await _service.UpdateAsync(weddingId, id, request);
        return result != null ? Ok(result) : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid weddingId, Guid id)
    {
        return await _service.DeleteAsync(weddingId, id) ? NoContent() : NotFound();
    }

    [HttpGet("stats")]
    public async Task<ActionResult<RsvpStatsDto>> GetStats(Guid weddingId)
        => Ok(await _service.GetStatsAsync(weddingId));
}
