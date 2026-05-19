using JobPortal.API.Data;
using JobPortal.API.Models;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.API.Repositories;

public class ResumeRepository : IResumeRepository
{
    private readonly AppDbContext _context;

    public ResumeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Resume>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Resumes
            .AsNoTracking()
            .OrderByDescending(r => r.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Resume?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Resumes.FindAsync([id], cancellationToken);
    }

    public async Task AddAsync(Resume entity, CancellationToken cancellationToken = default)
    {
        await _context.Resumes.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Resume entity, CancellationToken cancellationToken = default)
    {
        _context.Resumes.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Resumes.FindAsync([id], cancellationToken);
        if (entity == null)
        {
            return false;
        }

        _context.Resumes.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
