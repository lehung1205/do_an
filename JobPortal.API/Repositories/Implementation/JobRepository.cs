using JobPortal.API.Data;
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
                j.PostingStatus == "recruiting" &&
                j.ExpiryDate < now)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(j => j.PostingStatus, "closed"),
                cancellationToken);
    }

    public async Task<(IReadOnlyList<Job> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        bool recruitingOnly = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Jobs.AsNoTracking();
        if (recruitingOnly)
        {
            query = query.Where(j => j.PostingStatus == "recruiting");
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(j => j.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(j => j.Images.OrderBy(i => i.Id).Take(1))
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Job?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Jobs
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