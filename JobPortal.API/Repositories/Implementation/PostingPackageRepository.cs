using JobPortal.API.Data;
using JobPortal.API.Models;
using JobPortal.API.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.API.Repositories.Implementation;

public class PostingPackageRepository : IPostingPackageRepository
{
    private readonly AppDbContext _context;

    public PostingPackageRepository(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<PostingPackage>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PostingPackages
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Price)
            .ThenBy(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<(PostingPackage Package, int PaymentCount)>> GetAllWithPaymentCountsAsync(
        CancellationToken cancellationToken = default)
    {
        var rows = await _context.PostingPackages
            .AsNoTracking()
            .OrderByDescending(p => p.IsActive)
            .ThenBy(p => p.Price)
            .ThenByDescending(p => p.Id)
            .Select(p => new
            {
                Package = p,
                PaymentCount = p.PaymentHistories.Count
            })
            .ToListAsync(cancellationToken);

        return rows.Select(r => (r.Package, r.PaymentCount)).ToList();
    }

    public async Task<(PostingPackage Package, int PaymentCount)?> GetByIdWithPaymentCountAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var row = await _context.PostingPackages
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new
            {
                Package = p,
                PaymentCount = p.PaymentHistories.Count
            })
            .FirstOrDefaultAsync(cancellationToken);

        return row == null ? null : (row.Package, row.PaymentCount);
    }

    public async Task<PostingPackage?> GetByIdAsync(long id, CancellationToken cancellationToken = default) =>
        await _context.PostingPackages.FindAsync([id], cancellationToken);

    public async Task<int> GetPaymentCountAsync(long id, CancellationToken cancellationToken = default) =>
        await _context.PaymentHistories
            .AsNoTracking()
            .CountAsync(p => p.PostingPackageId == id, cancellationToken);

    public async Task<bool> ExistsByNameAsync(string name, long? excludeId, CancellationToken cancellationToken = default)
    {
        var query = _context.PostingPackages.AsNoTracking().Where(p => p.Name == name);
        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
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
