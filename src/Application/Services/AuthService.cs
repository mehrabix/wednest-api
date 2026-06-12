using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WedNest.Application.DTOs.Auth;
using WedNest.Application.Interfaces;
using WedNest.Domain.Entities;
using WedNest.Infrastructure.Data;

namespace WedNest.Application.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return new AuthResponse { Success = false, Message = "Email already registered" };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Role = (UserRole)request.Role,
            CreatedAt = DateTime.UtcNow
        };

        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken.Token;
        user.RefreshTokenExpiry = refreshToken.Expiry;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            Success = true,
            Message = "Registration successful",
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            AccessTokenExpiry = refreshToken.Expiry,
            RefreshTokenExpiry = refreshToken.Expiry,
            User = MapToDto(user)
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return new AuthResponse { Success = false, Message = "Invalid email or password" };

        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken.Token;
        user.RefreshTokenExpiry = refreshToken.Expiry;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            Success = true,
            Message = "Login successful",
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            AccessTokenExpiry = refreshToken.Expiry,
            RefreshTokenExpiry = refreshToken.Expiry,
            User = MapToDto(user)
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

        if (user == null)
            return new AuthResponse { Success = false, Message = "Invalid refresh token" };

        if (user.RefreshTokenExpiry == null || user.RefreshTokenExpiry < DateTime.UtcNow)
            return new AuthResponse { Success = false, Message = "Refresh token expired" };

        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken.Token;
        user.RefreshTokenExpiry = refreshToken.Expiry;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            Success = true,
            Message = "Token refreshed",
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            AccessTokenExpiry = refreshToken.Expiry,
            RefreshTokenExpiry = refreshToken.Expiry,
            User = MapToDto(user)
        };
    }

    public async Task<AuthResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        // Always return success to prevent email enumeration
        if (user == null)
            return new AuthResponse { Success = true, Message = "If the email exists, a reset link has been sent" };

        var resetToken = GeneratePasswordResetToken(user);

        // In production: send email with resetToken link
        // For now: return the token in response (dev only)
        return new AuthResponse
        {
            Success = true,
            Message = "If the email exists, a reset link has been sent",
            AccessToken = resetToken // Dev only - remove in production
        };
    }

    public async Task<AuthResponse> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
            return new AuthResponse { Success = false, Message = "Invalid reset request" };

        if (!ValidatePasswordResetToken(user, request.Token))
            return new AuthResponse { Success = false, Message = "Invalid or expired reset token" };

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            Success = true,
            Message = "Password reset successful"
        };
    }

    public async Task<AuthResponse> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return new AuthResponse { Success = false, Message = "User not found" };

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return new AuthResponse { Success = false, Message = "Current password is incorrect" };

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.RefreshToken = null; // Invalidate all existing refresh tokens
        user.RefreshTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            Success = true,
            Message = "Password changed successfully"
        };
    }

    public async Task<AuthResponse> RevokeRefreshTokenAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return new AuthResponse { Success = false, Message = "User not found" };

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            Success = true,
            Message = "Refresh token revoked"
        };
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        return user == null ? null : MapToDto(user);
    }

    // --- Private helpers ---

    private string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["JWT_SECRET"] ?? "your-super-secret-key-at-least-32-chars!!"));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var expiryMinutes = int.TryParse(_configuration["JWT_ACCESS_EXPIRY_MINUTES"], out var m) ? m : 15;

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT_ISSUER"] ?? "WedNest",
            audience: _configuration["JWT_AUDIENCE"] ?? "WedNest",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private (string Token, DateTime Expiry) GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        var expiryDays = int.TryParse(_configuration["JWT_REFRESH_EXPIRY_DAYS"], out var d) ? d : 7;

        return (
            Convert.ToBase64String(randomBytes),
            DateTime.UtcNow.AddDays(expiryDays)
        );
    }

    private string GeneratePasswordResetToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["JWT_SECRET"] ?? "your-super-secret-key-at-least-32-chars!!"));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("purpose", "password-reset")
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT_ISSUER"] ?? "WedNest",
            audience: _configuration["JWT_AUDIENCE"] ?? "WedNest",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private bool ValidatePasswordResetToken(User user, string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(
            _configuration["JWT_SECRET"] ?? "your-super-secret-key-at-least-32-chars!!");

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["JWT_ISSUER"] ?? "WedNest",
                ValidAudience = _configuration["JWT_AUDIENCE"] ?? "WedNest",
                IssuerSigningKey = new SymmetricSecurityKey(key)
            }, out _);

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var purpose = principal.FindFirst("purpose")?.Value;

            return userId == user.Id.ToString() && purpose == "password-reset";
        }
        catch
        {
            return false;
        }
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
