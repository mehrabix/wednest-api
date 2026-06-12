using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WedNest.Application.DTOs;
using WedNest.Application.Services;

namespace WedNest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LanguagesController : ControllerBase
{
    private readonly LanguageService _service;

    public LanguagesController(LanguageService service) => _service = service;

    [HttpGet]
    [EnableRateLimiting("public")]
    public async Task<ActionResult<List<LanguageDto>>> GetAll()
    {
        return await _service.GetAllAsync();
    }

    [HttpGet("{code}")]
    [EnableRateLimiting("public")]
    public async Task<ActionResult<LanguageDto>> GetByCode(string code)
    {
        var result = await _service.GetByCodeAsync(code);
        if (result == null) return NotFound();
        return result;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<LanguageDto>> Create([FromBody] CreateLanguageRequest request)
    {
        var result = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetByCode), new { code = result.Code }, result);
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await _service.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
