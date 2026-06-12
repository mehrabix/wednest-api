using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WedNest.Application.DTOs;
using WedNest.Application.DTOs.Auth;
using WedNest.Application.Services;

namespace WedNest.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _service;

    public UsersController(UserService service) => _service = service;

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetMe()
    {
        var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? User.FindFirstValue("email");
        if (string.IsNullOrEmpty(keycloakId)) return Unauthorized();

        var result = await _service.GetByKeycloakIdAsync(keycloakId);
        if (result == null) return NotFound();
        return result;
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserDto>> UpdateMe([FromBody] UserUpdateRequest request)
    {
        var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? User.FindFirstValue("email");
        if (string.IsNullOrEmpty(keycloakId)) return Unauthorized();

        var result = await _service.UpdateAsync(keycloakId, request);
        if (result == null) return NotFound();
        return result;
    }
}
