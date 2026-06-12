using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using WedNest.Application.DTOs;
using WedNest.Application.Services;

namespace WedNest.API.Controllers;

[ApiController]
[Route("api/public")]
public class PublicController : ControllerBase
{
    private readonly PublicService _service;
    private readonly RsvpService _rsvpService;

    public PublicController(PublicService service, RsvpService rsvpService)
    {
        _service = service;
        _rsvpService = rsvpService;
    }

    [HttpGet("weddings/{slug}")]
    [EnableRateLimiting("public")]
    public async Task<ActionResult<PublicWeddingDto>> GetWedding(string slug, [FromQuery] string? lang = null)
    {
        var result = await _service.GetWeddingBySlugAsync(slug, lang);
        if (result == null) return NotFound();
        return result;
    }

    [HttpPost("weddings/{slug}/rsvp")]
    [EnableRateLimiting("rsvp")]
    public async Task<ActionResult<GuestRsvpDto>> SubmitRsvp(string slug, [FromBody] CreateRsvpRequest request)
    {
        var wedding = await _service.GetWeddingIdBySlugAsync(slug);
        if (wedding == null) return NotFound();

        var result = await _rsvpService.CreateAsync(wedding.Value, request);
        return CreatedAtAction(nameof(GetWedding), new { slug }, result);
    }
}
