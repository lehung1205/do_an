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

    public async Task<(IReadOnlyList<Job> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Jobs.AsNoTracking();
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(j => j.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Job?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Jobs.FindAsync([id], cancellationToken);
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