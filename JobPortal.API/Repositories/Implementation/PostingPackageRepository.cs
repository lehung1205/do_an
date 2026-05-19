using JobPortal.API.Data;
using JobPortal.API.Repositories.Interface;
using JobPortal.API.Models;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.API.Repositories.Implementation;

public class PostingPackageRepository : IPostingPackageRepository
{
    private readonly AppDbContext _context;

    public PostingPackageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PostingPackage>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PostingPackages
            .AsNoTracking()
            .OrderByDescending(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<PostingPackage?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.PostingPackages.FindAsync([id], cancellationToken);
    }

    public async Task AddAsync(PostingPackage entity, CancellationToken cancellationToken = default)
    {
        await _context.PostingPackages.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(PostingPackage entity, CancellationToken cancellationToken = default)
    {
        _context.PostingPackages.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PostingPackages.FindAsync([id], cancellationToken);
        if (entity == null)
        {
            return false;
        }

        _context.PostingPackages.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}