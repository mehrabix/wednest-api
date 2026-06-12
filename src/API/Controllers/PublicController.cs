using Microsoft.AspNetCore.Mvc;
using WedNest.Application.DTOs;
using WedNest.Application.Services;

namespace WedNest.API.Controllers;

[ApiController]
[Route("api/public")]
public class PublicController : ControllerBase
{
    private readonly PublicService _service;

    public PublicController(PublicService service) => _service = service;

    [HttpGet("weddings/{slug}")]
    public async Task<ActionResult<PublicWeddingDto>> GetWedding(string slug, [FromQuery] string? lang = null)
    {
        var result = await _service.GetWeddingBySlugAsync(slug, lang);
        if (result == null) return NotFound();
        return result;
    }
}
