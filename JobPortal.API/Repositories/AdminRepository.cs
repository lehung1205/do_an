using JobPortal.API.Data;
using JobPortal.API.Models;
using JobPortal.API.Models.Auth;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.API.Repositories;

public class AdminRepository : IAdminRepository
{
    private readonly AppDbContext _context;

    public AdminRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Admin>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Admins
            .AsNoTracking()
            .OrderByDescending(a => a.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Admin?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Admins.FindAsync([id], cancellationToken);
    }

    public async Task<Admin?> GetByIdWithUserAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Admins
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<bool> UserExistsAsync(string email, string? phoneNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AnyAsync(
            u => u.Email == email || (phoneNumber != null && u.PhoneNumber == phoneNumber),
            cancellationToken);
    }

    public async Task AddWithUserAsync(User user, Admin admin, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        admin.UserId = user.Id;
        await _context.Admins.AddAsync(admin, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Admin admin, User user, CancellationToken cancellationToken = default)
    {
        _context.Admins.Update(admin);
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteWithUserAsync(Admin admin, CancellationToken cancellationToken = default)
    {
        _context.Admins.Remove(admin);

        var user = admin.User ?? await _context.Users.FindAsync([admin.UserId], cancellationToken);
        if (user != null)
        {
            _context.Users.Remove(user);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
