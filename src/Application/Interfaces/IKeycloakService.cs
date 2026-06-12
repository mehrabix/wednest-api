using WedNest.Application.DTOs.Auth;

namespace WedNest.Application.Interfaces;

public interface IKeycloakService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> RefreshAsync(RefreshRequest request);
    Task LogoutAsync(LogoutRequest request);
    Task<UserDto?> GetUserInfoAsync(string accessToken);
}
