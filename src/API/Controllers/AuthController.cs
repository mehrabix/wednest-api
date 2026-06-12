using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WedNest.Application.DTOs.Auth;
using WedNest.Application.Interfaces;

namespace WedNest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Get current user profile from Keycloak token (auto-creates in DB if new)
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _authService.GetOrCreateUserFromClaimsAsync(User);
        return user != null ? Ok(user) : Unauthorized();
    }

    /// <summary>
    /// Health check - no auth required
    /// </summary>
    [AllowAnonymous]
    [HttpGet("health")]
    public IActionResult Health() => Ok(new { status = "healthy", auth = "keycloak" });
}
