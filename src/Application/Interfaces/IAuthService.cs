using WedNest.Application.DTOs.Auth;

namespace WedNest.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
    Task<AuthResponse> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<AuthResponse> ResetPasswordAsync(ResetPasswordRequest request);
    Task<AuthResponse> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    Task<AuthResponse> RevokeRefreshTokenAsync(Guid userId);
    Task<UserDto?> GetUserByIdAsync(Guid userId);
}
