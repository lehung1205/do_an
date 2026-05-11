using JobPortal.API.Data;
using JobPortal.API.Models;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.API.Repositories;

public class CongViecRepository : ICongViecRepository
{
    private readonly AppDbContext _context;

    public CongViecRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CongViec>> GetAllAsync()
    {
        return await _context.CongViecs.AsNoTracking().ToListAsync();
    }

    public async Task<CongViec?> GetByIdAsync(long id)
    {
        return await _context.CongViecs.FindAsync(id);
    }

    public async Task AddAsync(CongViec entity)
    {
        await _context.CongViecs.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(CongViec entity)
    {
        _context.CongViecs.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _context.CongViecs.FindAsync(id);
        if (entity != null)
        {
            _context.CongViecs.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
