using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using JobPortal.API.DTOs.Auth;
using JobPortal.API.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace JobPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthLimiter")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.StartRegistrationAsync(request, cancellationToken);
        return Ok(ApiResponse<RegisterPendingResponse>.SuccessResponse(result, result.Message));
    }

    [HttpPost("register/verify")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthLimiter")]
    public async Task<IActionResult> VerifyRegister([FromBody] VerifyRegisterRequest request, CancellationToken cancellationToken)
    {
        var authResponse = await _authService.CompleteRegistrationAsync(request, GetClientIp(), cancellationToken);
        return Ok(ApiResponse<AuthResponse>.SuccessResponse(authResponse, "Đăng ký và xác minh email thành công."));
    }

    [HttpPost("register/resend-otp")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthLimiter")]
    public async Task<IActionResult> ResendRegisterOtp([FromBody] ResendRegisterOtpRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.ResendRegistrationOtpAsync(request, cancellationToken);
        return Ok(ApiResponse<RegisterPendingResponse>.SuccessResponse(result, result.Message));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthLimiter")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var authResponse = await _authService.LoginAsync(request, GetClientIp(), cancellationToken);
        return Ok(ApiResponse<AuthResponse>.SuccessResponse(authResponse, "Login successful."));
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var authResponse = await _authService.RefreshTokenAsync(request.Token, GetClientIp(), cancellationToken);
        return Ok(ApiResponse<AuthResponse>.SuccessResponse(authResponse, "Token refreshed successfully."));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RevokeTokenRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        await _authService.LogoutAsync(request.Token, userId, GetClientIp(), cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Logout successful."));
    }

    [HttpPost("revoke-token")]
    [Authorize]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        await _authService.RevokeTokenAsync(request.Token, userId, GetClientIp(), cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Token revoked successfully."));
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        await _authService.ChangePasswordAsync(userId, request, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Password changed successfully."));
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthLimiter")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var resetToken = await _authService.ForgotPasswordAsync(request, cancellationToken);
        var data = new { Message = "If this email exists, a password reset link has been sent.", ResetToken = resetToken };
        return Ok(ApiResponse<object>.SuccessResponse(data, "If the account exists, reset instructions have been issued."));
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        await _authService.ResetPasswordAsync(request, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Password reset successfully."));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var profile = await _authService.GetProfileAsync(userId, cancellationToken);
        return Ok(ApiResponse<ProfileResponse>.SuccessResponse(profile, "Current user profile retrieved."));
    }

    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var profile = await _authService.UpdateProfileAsync(userId, request, cancellationToken);
        return Ok(ApiResponse<ProfileResponse>.SuccessResponse(profile, "Profile updated successfully."));
    }

    private string GetClientIp()
    {
        return HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private long GetCurrentUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(sub, out var userId))
        {
            throw new UnauthorizedAccessException("User identifier is missing.");
        }

        return userId;
    }
}
