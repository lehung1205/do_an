using System.Security.Cryptography;
using JobPortal.API.Configurations;
using JobPortal.API.DTOs.Auth;
using JobPortal.API.Exceptions;
using JobPortal.API.Models;
using JobPortal.API.Models.Auth;
using JobPortal.API.Repositories;
using JobPortal.API.Services.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace JobPortal.API.Services.Auth;

public sealed class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly ITokenService _tokenService;
    private readonly JwtSettings _jwtSettings;
    private readonly PasswordHasher<User> _passwordHasher;

    private const string GenericAuthenticationError = "Email or password is invalid.";

    public AuthService(
        IAuthRepository authRepository,
        ITokenService tokenService,
        IOptions<JwtSettings> jwtSettings)
    {
        _authRepository = authRepository;
        _tokenService = tokenService;
        _jwtSettings = jwtSettings.Value;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, string ipAddress, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        if (await UserExistsAsync(normalizedEmail, request.PhoneNumber, cancellationToken))
        {
            throw new ConflictException("A user with the provided credentials already exists.");
        }

        var role = NormalizeRole(request.Role);
        var passwordHash = _passwordHasher.HashPassword(null!, request.Password);
        var now = DateTime.UtcNow;
        var authUser = new User
        {
            Name = request.Name.Trim(),
            Email = normalizedEmail,
            PhoneNumber = request.PhoneNumber?.Trim(),
            PasswordHash = passwordHash,
            Role = role,
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        };

        await _authRepository.AddUserAsync(authUser, cancellationToken);
        await _authRepository.SaveChangesAsync(cancellationToken);

        if (role == "EMPLOYER")
        {
            await _authRepository.AddEmployerAsync(new Employer
            {
                Name = authUser.Name,
                Email = authUser.Email,
                Phone = authUser.PhoneNumber,
                PasswordHash = authUser.PasswordHash,
                Status = "ACTIVE",
                Role = role,
                PostingLimit = 10,
                UserId = authUser.Id,
                CreatedAt = now,
                UpdatedAt = now
            }, cancellationToken);
        }
        else
        {
            await _authRepository.AddJobSeekerAsync(new JobSeeker
            {
                Name = authUser.Name,
                Email = authUser.Email,
                Phone = authUser.PhoneNumber,
                PasswordHash = authUser.PasswordHash,
                Status = "ACTIVE",
                Role = role,
                UserId = authUser.Id,
                CreatedAt = now,
                UpdatedAt = now
            }, cancellationToken);
        }

        await _authRepository.SaveChangesAsync(cancellationToken);

        var accessToken = _tokenService.CreateAccessToken(authUser);
        var (refreshTokenEntity, refreshTokenValue) = _tokenService.GenerateRefreshToken(ipAddress);
        refreshTokenEntity.UserId = authUser.Id;

        await _authRepository.AddRefreshTokenAsync(refreshTokenEntity, cancellationToken);
        await _authRepository.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            RefreshTokenExpiresAt = refreshTokenEntity.ExpiresAt,
            User = MapProfile(authUser)
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress, CancellationToken cancellationToken = default)
    {
        var user = await _authRepository.FindUserByIdentifierAsync(request.Identifier.Trim(), cancellationToken);
        if (user == null)
        {
            throw new UnauthorizedAccessException(GenericAuthenticationError);
        }

        var validation = _passwordHasher.VerifyHashedPassword(null!, user.PasswordHash, request.Password);
        if (validation == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException(GenericAuthenticationError);
        }

        var accessToken = _tokenService.CreateAccessToken(user);
        var (refreshTokenEntity, refreshTokenValue) = _tokenService.GenerateRefreshToken(ipAddress);
        refreshTokenEntity.UserId = user.Id;

        await _authRepository.AddRefreshTokenAsync(refreshTokenEntity, cancellationToken);
        await _authRepository.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            RefreshTokenExpiresAt = refreshTokenEntity.ExpiresAt,
            User = MapProfile(user)
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default)
    {
        var hashedToken = _tokenService.HashToken(refreshToken);
        var token = await _authRepository.FindRefreshTokenByHashAsync(hashedToken, cancellationToken);

        if (token == null)
        {
            throw new UnauthorizedAccessException("Refresh token is invalid.");
        }

        if (token.IsRevoked || token.IsUsed || token.IsExpired)
        {
            await RevokeAllTokensForUserAsync(token.UserId, ipAddress, cancellationToken);
            throw new UnauthorizedAccessException("Refresh token is invalid.");
        }

        token.ReplacedAt = DateTime.UtcNow;
        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;

        var (newRefreshTokenEntity, tokenValue) = _tokenService.GenerateRefreshToken(ipAddress);
        newRefreshTokenEntity.UserId = token.UserId;
        token.ReplacedByToken = newRefreshTokenEntity.TokenHash;
        token.User!.RefreshTokens.Add(newRefreshTokenEntity);

        await _authRepository.SaveChangesAsync(cancellationToken);

        var accessToken = _tokenService.CreateAccessToken(token.User);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = tokenValue,
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            RefreshTokenExpiresAt = newRefreshTokenEntity.ExpiresAt,
            User = MapProfile(token.User)
        };
    }

    public async Task LogoutAsync(string refreshToken, long userId, string ipAddress, CancellationToken cancellationToken = default)
    {
        await RevokeTokenInternalAsync(refreshToken, userId, ipAddress, cancellationToken);
    }

    public async Task RevokeTokenAsync(string token, long userId, string ipAddress, CancellationToken cancellationToken = default)
    {
        await RevokeTokenInternalAsync(token, userId, ipAddress, cancellationToken);
    }

    public async Task ChangePasswordAsync(long userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _authRepository.FindUserByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        var validation = _passwordHasher.VerifyHashedPassword(null!, user.PasswordHash, request.CurrentPassword);
        if (validation == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Current password is invalid.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(null!, request.NewPassword);

        await _authRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<string> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        var user = await _authRepository.FindUserByEmailAsync(normalizedEmail, cancellationToken);
        if (user == null)
        {
            return string.Empty;
        }

        var resetToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        user.PasswordResetTokenHash = _tokenService.HashToken(resetToken);
        user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ResetPasswordTokenExpirationMinutes);
        user.UpdatedAt = DateTime.UtcNow;

        await _authRepository.SaveChangesAsync(cancellationToken);
        return resetToken;
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            throw new UnauthorizedAccessException("Reset token is required.");
        }

        var hashedToken = _tokenService.HashToken(request.Token);
        var user = await _authRepository.FindUserByPasswordResetTokenHashAsync(hashedToken, cancellationToken);

        if (user == null || user.PasswordResetTokenExpiresAt == null || user.PasswordResetTokenExpiresAt < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Reset token is invalid or has expired.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(null!, request.NewPassword);
        user.PasswordResetTokenHash = null;
        user.PasswordResetTokenExpiresAt = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _authRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProfileResponse> GetProfileAsync(long userId, CancellationToken cancellationToken = default)
    {
        var user = await FindUserByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        return MapProfile(user);
    }

    private Task<bool> UserExistsAsync(string email, string? phoneNumber, CancellationToken cancellationToken)
    {
        return _authRepository.UserExistsAsync(email, phoneNumber, cancellationToken);
    }

    private Task<User?> FindUserByIdentifierAsync(string identifier, CancellationToken cancellationToken)
    {
        return _authRepository.FindUserByIdentifierAsync(identifier, cancellationToken);
    }

    private Task<User?> FindUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return _authRepository.FindUserByEmailAsync(email, cancellationToken);
    }

    private Task<User?> FindUserByIdAsync(long id, CancellationToken cancellationToken)
    {
        return _authRepository.FindUserByIdAsync(id, cancellationToken);
    }

    private async Task RevokeTokenInternalAsync(string token, long userId, string ipAddress, CancellationToken cancellationToken)
    {
        var hashedToken = _tokenService.HashToken(token);
        var refreshToken = await _authRepository.FindRefreshTokenByHashAsync(hashedToken, cancellationToken);

        if (refreshToken != null && refreshToken.UserId == userId)
        {
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            await _authRepository.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task RevokeAllTokensForUserAsync(long userId, string ipAddress, CancellationToken cancellationToken)
    {
        var tokens = await _authRepository.GetNonRevokedRefreshTokensForUserAsync(userId, cancellationToken);

        foreach (var token in tokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
        }

        await _authRepository.SaveChangesAsync(cancellationToken);
    }

    private ProfileResponse MapProfile(User user)
    {
        return new ProfileResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role
        };
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static string NormalizeRole(string role)
    {
        return role.Trim().ToUpperInvariant() switch
        {
            "JOB_SEEKER" => "JOB_SEEKER",
            "EMPLOYER" => "EMPLOYER",
            _ => throw new BadRequestException("Account type must be JOB_SEEKER or EMPLOYER.")
        };
    }
}
