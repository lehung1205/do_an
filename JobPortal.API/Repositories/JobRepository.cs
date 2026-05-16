using JobPortal.API.Data;
using JobPortal.API.Models;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.API.Repositories;

public class JobRepository : IJobRepository
{
    private readonly AppDbContext _context;

    public JobRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Job>> GetAllAsync()
    {
        return await _context.Jobs.AsNoTracking().ToListAsync();
    }

    public async Task<Job?> GetByIdAsync(long id)
    {
        return await _context.Jobs.FindAsync(id);
    }

    public async Task AddAsync(Job entity)
    {
        await _context.Jobs.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Job entity)
    {
        _context.Jobs.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _context.Jobs.FindAsync(id);
        if (entity != null)
        {
            _context.Jobs.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
