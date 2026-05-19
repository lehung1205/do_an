using JobPortal.API.Data;
using JobPortal.API.Models;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.API.Repositories;

public class WorkExperienceRepository : IWorkExperienceRepository
{
    private readonly AppDbContext _context;

    public WorkExperienceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<WorkExperience>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.WorkExperiences
            .AsNoTracking()
            .OrderByDescending(w => w.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkExperience?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.WorkExperiences.FindAsync([id], cancellationToken);
    }

    public async Task AddAsync(WorkExperience entity, CancellationToken cancellationToken = default)
    {
        await _context.WorkExperiences.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(WorkExperience entity, CancellationToken cancellationToken = default)
    {
        _context.WorkExperiences.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.WorkExperiences.FindAsync([id], cancellationToken);
        if (entity == null)
        {
            return false;
        }

        _context.WorkExperiences.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
