using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WedNest.Application.DTOs;
using WedNest.Application.Interfaces;
using WedNest.Application.Services;

namespace WedNest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeddingsController : ControllerBase
{
    private readonly WeddingService _service;
    private readonly IAuthService _authService;

    public WeddingsController(WeddingService service, IAuthService authService)
    {
        _service = service;
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result != null ? Ok(result) : NotFound();
    }

    [AllowAnonymous]
    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var result = await _service.GetBySlugAsync(slug);
        return result != null ? Ok(result) : NotFound();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWeddingRequest request)
    {
        var user = await _authService.GetOrCreateUserFromClaimsAsync(User);
        if (user == null) return Unauthorized();
        request.Partner1Id = user.Id;
        var result = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWeddingRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return result != null ? Ok(result) : NotFound();
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await _service.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
