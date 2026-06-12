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
    private readonly IKeycloakService _keycloakService;

    public AuthController(IAuthService authService, IKeycloakService keycloakService)
    {
        _authService = authService;
        _keycloakService = keycloakService;
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(typeof(AuthResponse), 401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _keycloakService.LoginAsync(request);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(typeof(AuthResponse), 400)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _keycloakService.RegisterAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(typeof(AuthResponse), 401)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var result = await _keycloakService.RefreshAsync(request);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    /// <summary>
    /// Logout (revoke refresh token)
    /// </summary>
    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        await _keycloakService.LogoutAsync(request);
        return Ok(new { message = "Logged out" });
    }

    /// <summary>
    /// Get current user profile from JWT token (auto-creates in DB if new)
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
