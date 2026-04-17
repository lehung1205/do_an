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
        return await _context.Jobs.ToListAsync();
    }

    public async Task<Job?> GetByIdAsync(int id)
    {
        return await _context.Jobs.FindAsync(id);
    }

    public async Task AddAsync(Job job)
    {
        await _context.Jobs.AddAsync(job);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Job job)
    {
        _context.Jobs.Update(job);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var job = await _context.Jobs.FindAsync(id);
        if (job != null)
        {
            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();
        }
    }
}
