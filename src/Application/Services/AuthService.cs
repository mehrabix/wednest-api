using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WedNest.Application.DTOs.Auth;
using WedNest.Application.Interfaces;
using WedNest.Domain.Entities;
using WedNest.Infrastructure.Data;

namespace WedNest.Application.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;

    public AuthService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserDto?> GetOrCreateUserFromClaimsAsync(ClaimsPrincipal principal)
    {
        var keycloakId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst("sub")?.Value
            ?? principal.FindFirst("email")?.Value
            ?? principal.FindFirst("preferred_username")?.Value;
        if (string.IsNullOrEmpty(keycloakId)) return null;

        var user = await _context.Users.FirstOrDefaultAsync(u => u.KeycloakId == keycloakId);
        if (user != null) return MapToDto(user);

        var email = principal.FindFirst(ClaimTypes.Email)?.Value
            ?? principal.FindFirst("email")?.Value;
        var firstName = principal.FindFirst(ClaimTypes.GivenName)?.Value
            ?? principal.FindFirst("given_name")?.Value ?? "";
        var lastName = principal.FindFirst(ClaimTypes.Surname)?.Value
            ?? principal.FindFirst("family_name")?.Value ?? "";
        var roleClaim = principal.FindFirst(ClaimTypes.Role)?.Value
            ?? principal.FindFirst("role")?.Value ?? "user";

        if (string.IsNullOrEmpty(email)) return null;

        user = new User
        {
            Id = Guid.NewGuid(),
            KeycloakId = keycloakId,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Role = roleClaim == "admin" ? UserRole.Admin : UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return MapToDto(user);
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        return user == null ? null : MapToDto(user);
    }

    public async Task<UserDto?> GetUserByKeycloakIdAsync(string keycloakId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.KeycloakId == keycloakId);
        return user == null ? null : MapToDto(user);
    }

    private static UserDto MapToDto(User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        PhoneNumber = user.PhoneNumber,
        Role = (int)user.Role
    };
}
