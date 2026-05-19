using JobPortal.API.Data;
using JobPortal.API.Models;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.API.Repositories;

public class ProcessRepository : IProcessRepository
{
    private readonly AppDbContext _context;

    public ProcessRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Process>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Processes
            .AsNoTracking()
            .OrderByDescending(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Process?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Processes.FindAsync([id], cancellationToken);
    }

    public async Task AddAsync(Process entity, CancellationToken cancellationToken = default)
    {
        await _context.Processes.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Process entity, CancellationToken cancellationToken = default)
    {
        _context.Processes.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Processes.FindAsync([id], cancellationToken);
        if (entity == null)
        {
            return false;
        }

        _context.Processes.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
