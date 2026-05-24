using System.Security.Cryptography;
using System.Text;
using JobPortal.API.Configurations;
using JobPortal.API.Data;
using JobPortal.API.DTOs.Auth;
using JobPortal.API.Exceptions;
using JobPortal.API.Models;
using JobPortal.API.Models.Auth;
using JobPortal.API.Repositories.Interface;
using JobPortal.API.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace JobPortal.API.Services.Implementation;

public sealed class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly ITokenService _tokenService;
    private readonly IEmailSender _emailSender;
    private readonly AppDbContext _context;
    private readonly IHostEnvironment _environment;
    private readonly EmailSettings _emailSettings;
    private readonly JwtSettings _jwtSettings;
    private readonly OtpSettings _otpSettings;
    private readonly PasswordHasher<User> _passwordHasher;

    private const string GenericAuthenticationError = "Email or password is invalid.";

    public AuthService(
        IAuthRepository authRepository,
        ITokenService tokenService,
        IEmailSender emailSender,
        AppDbContext context,
        IHostEnvironment environment,
        IOptions<EmailSettings> emailSettings,
        IOptions<JwtSettings> jwtSettings,
        IOptions<OtpSettings> otpSettings)
    {
        _authRepository = authRepository;
        _tokenService = tokenService;
        _emailSender = emailSender;
        _context = context;
        _environment = environment;
        _emailSettings = emailSettings.Value;
        _jwtSettings = jwtSettings.Value;
        _otpSettings = otpSettings.Value;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<RegisterPendingResponse> StartRegistrationAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        if (await UserExistsAsync(normalizedEmail, request.PhoneNumber, cancellationToken))
        {
            throw new ConflictException("Email hoặc số điện thoại đã được sử dụng.");
        }

        var role = NormalizeRole(request.Role);
        var passwordHash = _passwordHasher.HashPassword(null!, request.Password);
        var now = DateTime.UtcNow;
        var otpCode = GenerateOtpCode();
        var registrationToken = Guid.NewGuid().ToString("N");

        var existingPending = await _context.PendingRegistrations
            .Where(p => p.Email == normalizedEmail)
            .ToListAsync(cancellationToken);

        if (existingPending.Count > 0)
        {
            _context.PendingRegistrations.RemoveRange(existingPending);
        }

        var pending = new PendingRegistration
        {
            RegistrationToken = registrationToken,
            Email = normalizedEmail,
            Name = request.Name.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            Role = role,
            PasswordHash = passwordHash,
            OtpHash = _tokenService.HashToken(otpCode),
            OtpExpiresAt = now.AddMinutes(_otpSettings.ExpirationMinutes),
            FailedAttempts = 0,
            LastOtpSentAt = now,
            ExpiresAt = now.AddMinutes(_otpSettings.PendingRegistrationMinutes),
            CreatedAt = now
        };

        _context.PendingRegistrations.Add(pending);
        await _context.SaveChangesAsync(cancellationToken);

        await SendRegistrationOtpEmailAsync(normalizedEmail, otpCode, cancellationToken);

        return BuildRegisterPendingResponse(
            registrationToken,
            normalizedEmail,
            pending.OtpExpiresAt,
            otpCode,
            "Mã OTP đã được gửi tới email của bạn. Vui lòng xác minh trong vòng 10 phút.");
    }

    public async Task<RegisterPendingResponse> ResendRegistrationOtpAsync(
        ResendRegisterOtpRequest request,
        CancellationToken cancellationToken = default)
    {
        var pending = await FindPendingRegistrationAsync(request.RegistrationToken, cancellationToken);
        if (pending.ExpiresAt < DateTime.UtcNow)
        {
            throw new BadRequestException("Phiên đăng ký đã hết hạn. Vui lòng đăng ký lại.");
        }

        var secondsSinceLastSend = (DateTime.UtcNow - pending.LastOtpSentAt).TotalSeconds;
        if (secondsSinceLastSend < _otpSettings.ResendCooldownSeconds)
        {
            var wait = (int)Math.Ceiling(_otpSettings.ResendCooldownSeconds - secondsSinceLastSend);
            throw new BadRequestException($"Vui lòng đợi {wait} giây trước khi gửi lại mã.");
        }

        var otpCode = GenerateOtpCode();
        var now = DateTime.UtcNow;
        pending.OtpHash = _tokenService.HashToken(otpCode);
        pending.OtpExpiresAt = now.AddMinutes(_otpSettings.ExpirationMinutes);
        pending.LastOtpSentAt = now;
        pending.FailedAttempts = 0;

        await _context.SaveChangesAsync(cancellationToken);
        await SendRegistrationOtpEmailAsync(pending.Email, otpCode, cancellationToken);

        return BuildRegisterPendingResponse(
            pending.RegistrationToken,
            pending.Email,
            pending.OtpExpiresAt,
            otpCode,
            "Mã OTP mới đã được gửi tới email của bạn.");
    }

    public async Task<AuthResponse> CompleteRegistrationAsync(
        VerifyRegisterRequest request,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        var pending = await FindPendingRegistrationAsync(request.RegistrationToken, cancellationToken);

        if (pending.ExpiresAt < DateTime.UtcNow)
        {
            throw new BadRequestException("Phiên đăng ký đã hết hạn. Vui lòng đăng ký lại.");
        }

        if (pending.OtpExpiresAt < DateTime.UtcNow)
        {
            throw new BadRequestException("Mã OTP đã hết hạn. Vui lòng gửi lại mã.");
        }

        if (pending.FailedAttempts >= _otpSettings.MaxVerifyAttempts)
        {
            throw new BadRequestException("Đã nhập sai quá số lần cho phép. Vui lòng đăng ký lại.");
        }

        var otpHash = _tokenService.HashToken(request.OtpCode.Trim());
        if (!string.Equals(pending.OtpHash, otpHash, StringComparison.Ordinal))
        {
            pending.FailedAttempts++;
            await _context.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedAccessException("Mã OTP không đúng.");
        }

        if (await UserExistsAsync(pending.Email, pending.PhoneNumber, cancellationToken))
        {
            _context.PendingRegistrations.Remove(pending);
            await _context.SaveChangesAsync(cancellationToken);
            throw new ConflictException("Email hoặc số điện thoại đã được sử dụng.");
        }

        var authUser = await CreateUserAccountAsync(pending, cancellationToken);
        _context.PendingRegistrations.Remove(pending);
        await _context.SaveChangesAsync(cancellationToken);

        return await IssueAuthResponseAsync(authUser, ipAddress, cancellationToken);
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

        EnsureAccountCanAuthenticate(user);

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

        EnsureAccountCanAuthenticate(token.User!);

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
        var user = await _authRepository.FindUserByIdWithProfilesAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        return MapProfile(user);
    }

    public async Task<ProfileResponse> UpdateProfileAsync(long userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _authRepository.FindUserByIdWithProfilesAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        var phone = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
        if (await _authRepository.UserExistsForOtherAsync(userId, user.Email, phone, cancellationToken))
        {
            throw new ConflictException("Email or phone number is already in use.");
        }

        var now = DateTime.UtcNow;
        user.Name = request.Name.Trim();
        user.PhoneNumber = phone;
        user.UpdatedAt = now;

        if (user.JobSeekerProfile != null)
        {
            var j = user.JobSeekerProfile;
            j.Name = user.Name;
            j.Phone = phone;
            j.DateOfBirth = request.DateOfBirth;
            j.Gender = request.Gender;
            j.Description = TrimOrNull(request.Description);
            j.PermanentAddress = TrimOrNull(request.PermanentAddress);
            j.TemporaryAddress = TrimOrNull(request.TemporaryAddress);
            j.IdCard = TrimOrNull(request.IdCard);
            j.IdCardIssueDate = TrimOrNull(request.IdCardIssueDate);
            j.IdCardIssuePlace = TrimOrNull(request.IdCardIssuePlace);
            j.BankName = TrimOrNull(request.BankName);
            j.AccountNumber = TrimOrNull(request.BankAccountNumber);
            j.UpdatedAt = now;
        }
        else if (user.EmployerProfile != null)
        {
            var e = user.EmployerProfile;
            e.Name = user.Name;
            e.Phone = phone;
            e.DateOfBirth = request.DateOfBirth;
            e.Gender = request.Gender;
            e.Description = TrimOrNull(request.Description);
            e.IdCard = TrimOrNull(request.IdCard);
            e.UpdatedAt = now;
        }
        else if (user.AdminProfile != null)
        {
            var a = user.AdminProfile;
            a.Name = user.Name;
            a.Phone = phone;
            a.BankName = TrimOrNull(request.BankName);
            a.AccountNumber = TrimOrNull(request.BankAccountNumber);
            a.UpdatedAt = now;
        }

        if (request.ProfileImage != null)
        {
            var imageUrl = string.IsNullOrWhiteSpace(request.ProfileImage) ? null : request.ProfileImage.Trim();
            if (imageUrl != null && imageUrl.Length > 500)
            {
                throw new BadRequestException("Profile image URL is too long.");
            }

            user.ProfileImage = imageUrl;
            if (user.JobSeekerProfile != null)
            {
                user.JobSeekerProfile.ProfileImage = imageUrl;
            }

            if (user.EmployerProfile != null)
            {
                user.EmployerProfile.Image = imageUrl;
            }
        }

        await _authRepository.SaveChangesAsync(cancellationToken);
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

    private static void EnsureAccountCanAuthenticate(User user)
    {
        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException(
                "Tài khoản đã bị vô hiệu hóa. Vui lòng liên hệ quản trị viên.");
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
        var profile = new ProfileResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            JobSeekerId = user.JobSeekerProfile?.Id,
            EmployerId = user.EmployerProfile?.Id,
            AdminId = user.AdminProfile?.Id,
            ProfileImage = user.ProfileImage
                ?? user.JobSeekerProfile?.ProfileImage
                ?? user.EmployerProfile?.Image,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };

        if (user.JobSeekerProfile != null)
        {
            var j = user.JobSeekerProfile;
            profile.AccountStatus = j.Status;
            profile.EmailVerifiedAt = j.EmailVerifiedAt;
            profile.DateOfBirth = j.DateOfBirth;
            profile.Gender = j.Gender;
            profile.Description = j.Description;
            profile.PermanentAddress = j.PermanentAddress;
            profile.TemporaryAddress = j.TemporaryAddress;
            profile.IdCard = j.IdCard;
            profile.IdCardIssueDate = j.IdCardIssueDate;
            profile.IdCardIssuePlace = j.IdCardIssuePlace;
            profile.BankName = j.BankName;
            profile.BankAccountNumber = j.AccountNumber;
            profile.ProfilePhone = j.Phone;
            profile.PostingLimit = null;
        }
        else if (user.EmployerProfile != null)
        {
            var e = user.EmployerProfile;
            profile.AccountStatus = e.Status;
            profile.EmailVerifiedAt = e.EmailVerifiedAt;
            profile.DateOfBirth = e.DateOfBirth;
            profile.Gender = e.Gender;
            profile.Description = e.Description;
            profile.IdCard = e.IdCard;
            profile.ProfilePhone = e.Phone;
            profile.PostingLimit = e.PostingLimit;
        }
        else if (user.AdminProfile != null)
        {
            var a = user.AdminProfile;
            profile.AccountStatus = a.Status;
            profile.BankName = a.BankName;
            profile.BankAccountNumber = a.AccountNumber;
            profile.ProfilePhone = a.Phone;
        }

        return profile;
    }

    private static string? TrimOrNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

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

    private async Task<PendingRegistration> FindPendingRegistrationAsync(
        string registrationToken,
        CancellationToken cancellationToken)
    {
        var token = registrationToken?.Trim() ?? string.Empty;
        var pending = await _context.PendingRegistrations
            .FirstOrDefaultAsync(p => p.RegistrationToken == token, cancellationToken);

        if (pending == null)
        {
            throw new NotFoundException("Không tìm thấy phiên đăng ký. Vui lòng đăng ký lại.");
        }

        return pending;
    }

    private async Task<User> CreateUserAccountAsync(PendingRegistration pending, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var verifiedAt = now;

        var authUser = new User
        {
            Name = pending.Name,
            Email = pending.Email,
            PhoneNumber = pending.PhoneNumber,
            PasswordHash = pending.PasswordHash,
            Role = pending.Role,
            CreatedAt = now,
            UpdatedAt = now,
            IsActive = true
        };

        await _authRepository.AddUserAsync(authUser, cancellationToken);
        await _authRepository.SaveChangesAsync(cancellationToken);

        if (pending.Role == "EMPLOYER")
        {
            await _authRepository.AddEmployerAsync(new Employer
            {
                Name = authUser.Name,
                Email = authUser.Email,
                Phone = authUser.PhoneNumber,
                PasswordHash = authUser.PasswordHash,
                Status = "ACTIVE",
                Role = pending.Role,
                PostingLimit = 3,
                UserId = authUser.Id,
                EmailVerifiedAt = verifiedAt,
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
                Role = pending.Role,
                UserId = authUser.Id,
                EmailVerifiedAt = verifiedAt,
                CreatedAt = now,
                UpdatedAt = now
            }, cancellationToken);
        }

        await _authRepository.SaveChangesAsync(cancellationToken);
        return await _authRepository.FindUserByIdWithProfilesAsync(authUser.Id, cancellationToken) ?? authUser;
    }

    private async Task<AuthResponse> IssueAuthResponseAsync(
        User authUser,
        string ipAddress,
        CancellationToken cancellationToken)
    {
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

    private string GenerateOtpCode()
    {
        var max = (int)Math.Pow(10, _otpSettings.CodeLength);
        var value = RandomNumberGenerator.GetInt32(0, max);
        return value.ToString($"D{_otpSettings.CodeLength}");
    }

    private Task SendRegistrationOtpEmailAsync(string email, string otpCode, CancellationToken cancellationToken)
    {
        var subject = "Mã xác minh đăng ký JobPortal";
        var body = new StringBuilder()
            .Append("<div style=\"font-family:Arial,sans-serif;max-width:480px\">")
            .Append("<h2 style=\"color:#4f46e5\">Xác minh đăng ký</h2>")
            .Append("<p>Mã OTP của bạn (hiệu lực ")
            .Append(_otpSettings.ExpirationMinutes)
            .Append(" phút):</p>")
            .Append("<p style=\"font-size:28px;font-weight:bold;letter-spacing:6px\">")
            .Append(otpCode)
            .Append("</p>")
            .Append("<p style=\"color:#64748b;font-size:13px\">Không chia sẻ mã này với ai. ")
            .Append("Sau khi xác minh, bạn đăng nhập bằng email và mật khẩu đã đặt.</p>")
            .Append("</div>")
            .ToString();

        return _emailSender.SendAsync(email, subject, body, otpCode, cancellationToken);
    }

    private RegisterPendingResponse BuildRegisterPendingResponse(
        string registrationToken,
        string email,
        DateTime otpExpiresAt,
        string otpCode,
        string message)
    {
        var response = new RegisterPendingResponse
        {
            RegistrationToken = registrationToken,
            Email = MaskEmail(email),
            OtpExpiresAt = otpExpiresAt,
            Message = message
        };

        if (_environment.IsDevelopment() && !_emailSettings.IsSmtpConfigured)
        {
            response.DevOtp = otpCode;
            response.Message = message + " (Dev: SMTP chưa bật — xem mã OTP trên màn hình hoặc log API.)";
        }

        return response;
    }

    private static string MaskEmail(string email)
    {
        var at = email.IndexOf('@');
        if (at <= 1)
        {
            return email;
        }

        var local = email[..at];
        var domain = email[at..];
        var visible = local.Length <= 2 ? local[0].ToString() : local[..2];
        return visible + new string('*', Math.Min(local.Length - 2, 4)) + domain;
    }
}
