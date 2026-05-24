using JobPortal.API.Data;
using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;
using JobPortal.API.Exceptions;
using JobPortal.API.Helpers;
using JobPortal.API.Repositories.Interface;
using JobPortal.API.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.API.Services.Implementation;

public class AdminUserManagementService : IAdminUserManagementService
{
    private const int MaxPageSize = 50;
    private readonly AppDbContext _context;
    private readonly IAuthRepository _authRepository;

    public AdminUserManagementService(AppDbContext context, IAuthRepository authRepository)
    {
        _context = context;
        _authRepository = authRepository;
    }

    public async Task<PagedResult<AdminManagedEmployerDto>> GetEmployersPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 || pageSize > MaxPageSize ? 12 : pageSize;

        var query = _context.Employers.AsNoTracking();

        var statusFilter = NormalizeStatusFilter(status);
        if (statusFilter != null)
        {
            query = query.Where(e => e.Status == statusFilter);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(e =>
                e.Name.Contains(term) ||
                e.Email.Contains(term) ||
                (e.Phone != null && e.Phone.Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(e => e.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new AdminManagedEmployerDto
            {
                Id = e.Id,
                UserId = e.UserId,
                Name = e.Name,
                Email = e.Email,
                Phone = e.Phone,
                Image = e.Image,
                Status = e.Status,
                PostingLimit = e.PostingLimit,
                JobCount = e.Jobs.Count,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return BuildPagedResult(items, page, pageSize, totalCount);
    }

    public async Task<PagedResult<AdminManagedJobSeekerDto>> GetJobSeekersPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 || pageSize > MaxPageSize ? 12 : pageSize;

        var query = _context.JobSeekers.AsNoTracking();

        var statusFilter = NormalizeStatusFilter(status);
        if (statusFilter != null)
        {
            query = query.Where(j => j.Status == statusFilter);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(j =>
                j.Name.Contains(term) ||
                j.Email.Contains(term) ||
                (j.Phone != null && j.Phone.Contains(term)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(j => j.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(j => new AdminManagedJobSeekerDto
            {
                Id = j.Id,
                UserId = j.UserId,
                Name = j.Name,
                Email = j.Email,
                Phone = j.Phone,
                ProfileImage = j.ProfileImage ?? (j.User != null ? j.User.ProfileImage : null),
                Status = j.Status,
                ApplicationCount = j.Applications.Count,
                CreatedAt = j.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return BuildPagedResult(items, page, pageSize, totalCount);
    }

    public async Task<AdminManagedEmployerDto> SetEmployerActiveAsync(
        long employerId,
        bool active,
        string? revokedByIp = null,
        CancellationToken cancellationToken = default)
    {
        var employer = await _context.Employers
            .Include(e => e.User)
            .Include(e => e.Jobs)
            .FirstOrDefaultAsync(e => e.Id == employerId, cancellationToken);

        if (employer == null)
        {
            throw new NotFoundException($"Employer with id {employerId} was not found.");
        }

        ApplyStatus(employer.User, active, out var status);
        employer.Status = status;
        employer.UpdatedAt = DateTime.UtcNow;

        if (!active && employer.User != null)
        {
            await RevokeRefreshTokensAsync(employer.User.Id, revokedByIp, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return MapEmployer(employer);
    }

    public async Task<AdminManagedJobSeekerDto> SetJobSeekerActiveAsync(
        long jobSeekerId,
        bool active,
        string? revokedByIp = null,
        CancellationToken cancellationToken = default)
    {
        var jobSeeker = await _context.JobSeekers
            .Include(j => j.User)
            .Include(j => j.Applications)
            .FirstOrDefaultAsync(j => j.Id == jobSeekerId, cancellationToken);

        if (jobSeeker == null)
        {
            throw new NotFoundException($"Job seeker with id {jobSeekerId} was not found.");
        }

        ApplyStatus(jobSeeker.User, active, out var status);
        jobSeeker.Status = status;
        jobSeeker.UpdatedAt = DateTime.UtcNow;

        if (!active && jobSeeker.User != null)
        {
            await RevokeRefreshTokensAsync(jobSeeker.User.Id, revokedByIp, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return MapJobSeeker(jobSeeker);
    }

    private static void ApplyStatus(Models.Auth.User? user, bool active, out string status)
    {
        status = AccountStatusCatalog.FromActiveFlag(active);
        if (user != null)
        {
            user.IsActive = active;
            user.UpdatedAt = DateTime.UtcNow;
        }
    }

    private async Task RevokeRefreshTokensAsync(
        long userId,
        string? revokedByIp,
        CancellationToken cancellationToken)
    {
        var tokens = (await _authRepository.GetNonRevokedRefreshTokensForUserAsync(userId, cancellationToken))
            .ToList();
        if (tokens.Count == 0)
        {
            return;
        }

        var ip = string.IsNullOrWhiteSpace(revokedByIp) ? "admin" : revokedByIp;
        var now = DateTime.UtcNow;

        foreach (var token in tokens)
        {
            token.RevokedAt = now;
            token.RevokedByIp = ip;
        }

        await _authRepository.SaveChangesAsync(cancellationToken);
    }

    private static string? NormalizeStatusFilter(string? status)
    {
        if (string.IsNullOrWhiteSpace(status) || string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var normalized = status.Trim().ToUpperInvariant();
        return normalized is AccountStatusCatalog.Active or AccountStatusCatalog.Inactive
            ? normalized
            : null;
    }

    private static PagedResult<T> BuildPagedResult<T>(IReadOnlyList<T> items, int page, int pageSize, int totalCount) =>
        new()
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize)
        };

    private static AdminManagedEmployerDto MapEmployer(Models.Employer employer) => new()
    {
        Id = employer.Id,
        UserId = employer.UserId,
        Name = employer.Name,
        Email = employer.Email,
        Phone = employer.Phone,
        Image = employer.Image,
        Status = employer.Status,
        PostingLimit = employer.PostingLimit,
        JobCount = employer.Jobs?.Count ?? 0,
        CreatedAt = employer.CreatedAt
    };

    private static AdminManagedJobSeekerDto MapJobSeeker(Models.JobSeeker jobSeeker) => new()
    {
        Id = jobSeeker.Id,
        UserId = jobSeeker.UserId,
        Name = jobSeeker.Name,
        Email = jobSeeker.Email,
        Phone = jobSeeker.Phone,
        ProfileImage = jobSeeker.ProfileImage ?? jobSeeker.User?.ProfileImage,
        Status = jobSeeker.Status,
        ApplicationCount = jobSeeker.Applications?.Count ?? 0,
        CreatedAt = jobSeeker.CreatedAt
    };
}
