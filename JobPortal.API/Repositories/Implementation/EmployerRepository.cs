using JobPortal.API.Data;
using JobPortal.API.Models;
using JobPortal.API.Models.Auth;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.API.Repositories.Implementation;

public class EmployerRepository : IEmployerRepository
{
    private readonly AppDbContext _context;

    public EmployerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Employer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Employers
            .AsNoTracking()
            .OrderByDescending(e => e.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Employer?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Employers.FindAsync([id], cancellationToken);
    }

    public async Task<Employer?> GetByIdWithUserAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Employers
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<bool> UserExistsAsync(string email, string? phoneNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AnyAsync(
            u => u.Email == email || (phoneNumber != null && u.PhoneNumber == phoneNumber),
            cancellationToken);
    }

    public async Task AddWithUserAsync(User user, Employer employer, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        employer.UserId = user.Id;
        await _context.Employers.AddAsync(employer, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Employer employer, User user, CancellationToken cancellationToken = default)
    {
        _context.Employers.Update(employer);
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteWithUserAsync(Employer employer, CancellationToken cancellationToken = default)
    {
        _context.Employers.Remove(employer);

        var user = employer.User ?? await _context.Users.FindAsync([employer.UserId], cancellationToken);
        if (user != null)
        {
            _context.Users.Remove(user);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}