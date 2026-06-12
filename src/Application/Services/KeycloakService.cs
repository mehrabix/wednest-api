using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using WedNest.Application.DTOs.Auth;
using WedNest.Application.Interfaces;

namespace WedNest.Application.Services;

public class KeycloakService : IKeycloakService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public KeycloakService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    private string TokenUrl => $"{_config["KEYCLOAK_URL"]}/realms/{_config["KEYCLOAK_REALM"]}/protocol/openid-connect/token";
    private string RegisterUrl => $"{_config["KEYCLOAK_URL"]}/realms/{_config["KEYCLOAK_REALM"]}/protocol/openid-connect/registrations";
    private string AdminUrl => $"{_config["KEYCLOAK_URL"]}/admin/realms/{_config["KEYCLOAK_REALM"]}";
    private string ClientId => _config["KEYCLOAK_CLIENT_ID"] ?? "";
    private string ClientSecret => _config["KEYCLOAK_CLIENT_SECRET"] ?? "";

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = ClientId,
            ["client_secret"] = ClientSecret,
            ["username"] = request.Email,
            ["password"] = request.Password,
            ["scope"] = "openid profile email"
        };

        var response = await _http.PostAsync(TokenUrl, new FormUrlEncodedContent(form));
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return new AuthResponse { Success = false, Message = "Invalid email or password" };

        var tokenResponse = JsonSerializer.Deserialize<JsonElement>(json);
        var user = await GetUserInfoAsync(tokenResponse.GetProperty("access_token").GetString()!);

        return new AuthResponse
        {
            Success = true,
            Message = "Login successful",
            AccessToken = tokenResponse.GetProperty("access_token").GetString(),
            RefreshToken = tokenResponse.GetProperty("refresh_token").GetString(),
            ExpiresIn = tokenResponse.GetProperty("expires_in").GetInt32(),
            User = user
        };
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var adminToken = await GetAdminTokenAsync();

        var userPayload = new
        {
            username = request.Email,
            email = request.Email,
            firstName = request.FirstName,
            lastName = request.LastName,
            enabled = true,
            emailVerified = true,
            credentials = new[] { new { type = "password", value = request.Password, temporary = false } }
        };

        var http = new HttpClient();
        http.DefaultRequestHeaders.Authorization = new("Bearer", adminToken);
        var response = await http.PostAsJsonAsync($"{AdminUrl}/users", userPayload);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return new AuthResponse { Success = false, Message = $"Registration failed: {response.StatusCode}" };
        }

        // Assign default role
        var userResp = await http.GetAsync($"{AdminUrl}/users?email={request.Email}");
        var users = await userResp.Content.ReadFromJsonAsync<List<JsonElement>>();
        if (users?.Count > 0)
        {
            var userId = users[0].GetProperty("id").GetString();
            var roleResp = await http.GetAsync($"{AdminUrl}/roles/user");
            var role = await roleResp.Content.ReadFromJsonAsync<JsonElement>();
            var roleId = role.GetProperty("id").GetString()!;
            var roleName = role.GetProperty("name").GetString()!;

            await http.PostAsJsonAsync($"{AdminUrl}/users/{userId}/role-mappings/realm",
                new[] { new { id = roleId, name = roleName } });
        }

        // Auto login after registration
        return await LoginAsync(new LoginRequest { Email = request.Email, Password = request.Password });
    }

    public async Task<AuthResponse> RefreshAsync(RefreshRequest request)
    {
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = ClientId,
            ["client_secret"] = ClientSecret,
            ["refresh_token"] = request.RefreshToken
        };

        var response = await _http.PostAsync(TokenUrl, new FormUrlEncodedContent(form));
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return new AuthResponse { Success = false, Message = "Invalid refresh token" };

        var tokenResponse = JsonSerializer.Deserialize<JsonElement>(json);
        var user = await GetUserInfoAsync(tokenResponse.GetProperty("access_token").GetString()!);

        return new AuthResponse
        {
            Success = true,
            Message = "Token refreshed",
            AccessToken = tokenResponse.GetProperty("access_token").GetString(),
            RefreshToken = tokenResponse.GetProperty("refresh_token").GetString(),
            ExpiresIn = tokenResponse.GetProperty("expires_in").GetInt32(),
            User = user
        };
    }

    public async Task LogoutAsync(LogoutRequest request)
    {
        if (string.IsNullOrEmpty(request.RefreshToken)) return;

        var form = new Dictionary<string, string>
        {
            ["client_id"] = ClientId,
            ["client_secret"] = ClientSecret,
            ["refresh_token"] = request.RefreshToken
        };

        await _http.PostAsync(TokenUrl.Replace("/token", "/logout"), new FormUrlEncodedContent(form));
    }

    private async Task<string> GetAdminTokenAsync()
    {
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = "admin-cli",
            ["username"] = _config["KEYCLOAK_ADMIN_USER"] ?? "ahmad",
            ["password"] = _config["KEYCLOAK_ADMIN_PASSWORD"] ?? "4203874"
        };

        var response = await _http.PostAsync(
            $"{_config["KEYCLOAK_URL"]}/realms/master/protocol/openid-connect/token",
            new FormUrlEncodedContent(form));

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<JsonElement>(json).GetProperty("access_token").GetString()!;
    }

    public async Task<UserDto?> GetUserInfoAsync(string accessToken)
    {
        var http = new HttpClient();
        http.DefaultRequestHeaders.Authorization = new("Bearer", accessToken);
        var response = await http.GetAsync($"{_config["KEYCLOAK_URL"]}/realms/{_config["KEYCLOAK_REALM"]}/protocol/openid-connect/userinfo");

        if (!response.IsSuccessStatusCode) return null;

        var userInfo = await response.Content.ReadFromJsonAsync<JsonElement>();
        return new UserDto
        {
            Id = Guid.NewGuid(),
            Email = userInfo.GetProperty("email").GetString() ?? "",
            FirstName = userInfo.TryGetProperty("given_name", out var fn) ? fn.GetString() ?? "" : "",
            LastName = userInfo.TryGetProperty("family_name", out var ln) ? ln.GetString() ?? "" : "",
            Role = 1
        };
    }
}
