using JobPortal.API.Data;
using JobPortal.API.Repositories.Interface;
using JobPortal.API.Models;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.API.Repositories.Implementation;

public class ReviewRepository : IReviewRepository
{
    private readonly AppDbContext _context;

    public ReviewRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Review>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Reviews
            .AsNoTracking()
            .OrderByDescending(r => r.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Review?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Reviews.FindAsync([id], cancellationToken);
    }

    public async Task AddAsync(Review entity, CancellationToken cancellationToken = default)
    {
        await _context.Reviews.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Review entity, CancellationToken cancellationToken = default)
    {
        _context.Reviews.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Reviews.FindAsync([id], cancellationToken);
        if (entity == null)
        {
            return false;
        }

        _context.Reviews.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}