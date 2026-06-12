using System.Security.Claims;
using WedNest.Application.DTOs.Auth;

namespace WedNest.Application.Interfaces;

public interface IAuthService
{
    Task<UserDto?> GetOrCreateUserFromClaimsAsync(ClaimsPrincipal principal);
    Task<UserDto?> GetUserByIdAsync(Guid userId);
    Task<UserDto?> GetUserByKeycloakIdAsync(string keycloakId);
}
