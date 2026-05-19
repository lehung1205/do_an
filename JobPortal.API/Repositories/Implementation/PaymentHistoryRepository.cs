using JobPortal.API.Data;
using JobPortal.API.Repositories.Interface;
using JobPortal.API.Models;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.API.Repositories.Implementation;

public class PaymentHistoryRepository : IPaymentHistoryRepository
{
    private readonly AppDbContext _context;

    public PaymentHistoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PaymentHistory>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PaymentHistories
            .AsNoTracking()
            .OrderByDescending(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<PaymentHistory?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.PaymentHistories.FindAsync([id], cancellationToken);
    }

    public async Task AddAsync(PaymentHistory entity, CancellationToken cancellationToken = default)
    {
        await _context.PaymentHistories.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(PaymentHistory entity, CancellationToken cancellationToken = default)
    {
        _context.PaymentHistories.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PaymentHistories.FindAsync([id], cancellationToken);
        if (entity == null)
        {
            return false;
        }

        _context.PaymentHistories.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}