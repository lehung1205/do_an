using JobPortal.API.Data;
using JobPortal.API.Repositories.Interface;
using JobPortal.API.Models;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.API.Repositories.Implementation;

public class ApplicationRepository : IApplicationRepository
{
    private readonly AppDbContext _context;

    public ApplicationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Application>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Applications
            .AsNoTracking()
            .OrderByDescending(a => a.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Application?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Applications.FindAsync([id], cancellationToken);
    }

    public async Task AddAsync(Application entity, CancellationToken cancellationToken = default)
    {
        await _context.Applications.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Application entity, CancellationToken cancellationToken = default)
    {
        _context.Applications.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Applications.FindAsync([id], cancellationToken);
        if (entity == null)
        {
            return false;
        }

        _context.Applications.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task DeleteByResumeIdAsync(long resumeId, CancellationToken cancellationToken = default)
    {
        var applications = await _context.Applications
            .Include(a => a.Processes)
            .Where(a => a.ResumeId == resumeId)
            .ToListAsync(cancellationToken);

        if (applications.Count == 0)
        {
            return;
        }

        _context.Applications.RemoveRange(applications);
        await _context.SaveChangesAsync(cancellationToken);
    }
}