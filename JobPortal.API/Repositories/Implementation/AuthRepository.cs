using JobPortal.API.Data;
using JobPortal.API.Repositories.Interface;
using JobPortal.API.Models;
using JobPortal.API.Models.Auth;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.API.Repositories.Implementation;

public class AuthRepository : IAuthRepository
{
    private readonly AppDbContext _context;

    public AuthRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> UserExistsAsync(string email, string? phoneNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AnyAsync(u => u.Email == email || u.PhoneNumber == phoneNumber, cancellationToken);
    }

    public async Task AddUserAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public async Task AddJobSeekerAsync(JobSeeker jobSeeker, CancellationToken cancellationToken = default)
    {
        await _context.JobSeekers.AddAsync(jobSeeker, cancellationToken);
    }

    public async Task AddEmployerAsync(Employer employer, CancellationToken cancellationToken = default)
    {
        await _context.Employers.AddAsync(employer, cancellationToken);
    }

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    public async Task<User?> FindUserByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
    {
        var normalizedIdentifier = identifier.Trim();
        var normalizedEmail = normalizedIdentifier.ToLowerInvariant();

        return await _context.Users
            .Include(u => u.AdminProfile)
            .Include(u => u.EmployerProfile)
            .Include(u => u.JobSeekerProfile)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail || u.PhoneNumber == normalizedIdentifier, cancellationToken);
    }

    public async Task<User?> FindUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> FindUserByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> FindUserByPasswordResetTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetTokenHash == tokenHash, cancellationToken);
    }

    public async Task<RefreshToken?> FindRefreshTokenByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);
    }

    public async Task<IEnumerable<RefreshToken>> GetNonRevokedRefreshTokensForUserAsync(long userId, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}