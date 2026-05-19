using JobPortal.API.Data;
using JobPortal.API.Repositories.Interface;
using JobPortal.API.Models;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.API.Repositories.Implementation;

public class ImageRepository : IImageRepository
{
    private readonly AppDbContext _context;

    public ImageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Image>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Images
            .AsNoTracking()
            .OrderByDescending(i => i.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Image?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Images.FindAsync([id], cancellationToken);
    }

    public async Task AddAsync(Image entity, CancellationToken cancellationToken = default)
    {
        await _context.Images.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Image entity, CancellationToken cancellationToken = default)
    {
        _context.Images.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Images.FindAsync([id], cancellationToken);
        if (entity == null)
        {
            return false;
        }

        _context.Images.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}