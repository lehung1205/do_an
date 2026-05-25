using JobPortal.API.Data;
using JobPortal.API.Helpers;
using JobPortal.API.Repositories.Interface;
using JobPortal.API.Models;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.API.Repositories.Implementation;

public class JobRepository : IJobRepository
{
    private readonly AppDbContext _context;

    public JobRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> CloseExpiredRecruitingJobsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Jobs
            .Where(j =>
                j.PostingStatus == JobPostingCatalog.Recruiting &&
                j.ExpiryDate < now)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(j => j.PostingStatus, JobPostingCatalog.Closed),
                cancellationToken);
    }

    public async Task<int> AutoApproveStalePendingJobsAsync(
        TimeSpan pendingMaxAge,
        CancellationToken cancellationToken = default)
    {
        var threshold = DateTime.UtcNow - pendingMaxAge;
        return await _context.Jobs
            .Where(j =>
                j.PostingStatus == JobPostingCatalog.Pending &&
                j.CreatedAt <= threshold)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(j => j.PostingStatus, JobPostingCatalog.Recruiting),
                cancellationToken);
    }

    public async Task<(IReadOnlyList<Job> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        bool recruitingOnly = false,
        string? search = null,
        string? location = null,
        long? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Jobs.AsNoTracking();
        if (recruitingOnly)
        {
            query = query.Where(j => j.PostingStatus == JobPostingCatalog.Recruiting);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(j =>
                j.Title.Contains(term) ||
                j.Description.Contains(term) ||
                j.Location.Contains(term) ||
                j.Salary.Contains(term) ||
                (j.WorkingHours != null && j.WorkingHours.Contains(term)) ||
                j.Employer.Name.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            var loc = location.Trim();
            query = query.Where(j => j.Location.Contains(loc));
        }

        if (categoryId is > 0)
        {
            query = query.Where(j => j.CategoryId == categoryId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(j => j.CreatedAt)
            .ThenByDescending(j => j.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(j => j.Employer)
            .Include(j => j.Category)
            .Include(j => j.Images.OrderBy(i => i.Id).Take(1))
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Job?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Jobs
            .Include(j => j.Employer)
            .Include(j => j.Images.OrderBy(i => i.Id).Take(1))
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
    }

    public async Task AddAsync(Job entity, CancellationToken cancellationToken = default)
    {
        await _context.Jobs.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Job entity, CancellationToken cancellationToken = default)
    {
        _context.Jobs.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Jobs.FindAsync([id], cancellationToken);
        if (entity == null)
        {
            return false;
        }

        _context.Jobs.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}