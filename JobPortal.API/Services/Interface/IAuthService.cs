using JobPortal.API.DTOs.Auth;

namespace JobPortal.API.Services.Interface;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, string ipAddress, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress, CancellationToken cancellationToken = default);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default);
    Task LogoutAsync(string refreshToken, long userId, string ipAddress, CancellationToken cancellationToken = default);
    Task RevokeTokenAsync(string token, long userId, string ipAddress, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(long userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task<string> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
    Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
    Task<ProfileResponse> GetProfileAsync(long userId, CancellationToken cancellationToken = default);
}
