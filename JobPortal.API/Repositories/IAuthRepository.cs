using JobPortal.API.Models;
using JobPortal.API.Models.Auth;

namespace JobPortal.API.Repositories;

public interface IAuthRepository
{
    Task<bool> UserExistsAsync(string email, string? phoneNumber, CancellationToken cancellationToken = default);
    Task AddUserAsync(User user, CancellationToken cancellationToken = default);
    Task AddJobSeekerAsync(JobSeeker jobSeeker, CancellationToken cancellationToken = default);
    Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task<User?> FindUserByIdentifierAsync(string identifier, CancellationToken cancellationToken = default);
    Task<User?> FindUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> FindUserByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<User?> FindUserByPasswordResetTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<RefreshToken?> FindRefreshTokenByHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<IEnumerable<RefreshToken>> GetNonRevokedRefreshTokensForUserAsync(long userId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
