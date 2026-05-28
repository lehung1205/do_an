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

    public async Task<bool> ExistsForJobSeekerAndJobAsync(
        long jobSeekerId,
        long jobId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Applications
            .AsNoTracking()
            .AnyAsync(a => a.JobSeekerId == jobSeekerId && a.JobId == jobId, cancellationToken);
    }

    public async Task<IReadOnlyList<Application>> GetByJobSeekerIdAsync(
        long jobSeekerId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Applications
            .AsNoTracking()
            .Where(a => a.JobSeekerId == jobSeekerId)
            .OrderByDescending(a => a.AppliedAt)
            .Include(a => a.Job)
                .ThenInclude(j => j.Employer)
            .Include(a => a.Job)
                .ThenInclude(j => j.Category)
            .Include(a => a.Job)
                .ThenInclude(j => j.Images)
            .Include(a => a.Resume)
            .ToListAsync(cancellationToken);
    }
}