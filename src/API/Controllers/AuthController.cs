using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WedNest.Application.DTOs;
using WedNest.Application.DTOs.Auth;
using WedNest.Application.Interfaces;
using WedNest.Application.Services;

namespace WedNest.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IKeycloakService _keycloakService;
    private readonly UserService _userService;

    public AuthController(IAuthService authService, IKeycloakService keycloakService, UserService userService)
    {
        _authService = authService;
        _keycloakService = keycloakService;
        _userService = userService;
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

    /// <summary>
    /// Get current user profile from DB
    /// </summary>
    [Authorize]
    [HttpGet("profile")]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetProfile()
    {
        var user = await _authService.GetOrCreateUserFromClaimsAsync(User);
        if (user == null) return NotFound();
        return Ok(user);
    }

    /// <summary>
    /// Update current user profile
    /// </summary>
    [Authorize]
    [HttpPut("profile")]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateProfile([FromBody] UserUpdateRequest request)
    {
        var keycloakId = User.FindFirstValue("sub")
            ?? User.FindFirstValue("email")
            ?? User.FindFirstValue("preferred_username");
        if (string.IsNullOrEmpty(keycloakId)) return Unauthorized();

        var result = await _userService.UpdateAsync(keycloakId, request);
        if (result == null) return NotFound();
        return Ok(result);
    }
}
