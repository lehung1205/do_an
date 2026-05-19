using JobPortal.API.Data;
using JobPortal.API.Repositories.Interface;
using JobPortal.API.Models;
using JobPortal.API.Models.Auth;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.API.Repositories.Implementation;

public class JobSeekerRepository : IJobSeekerRepository
{
    private readonly AppDbContext _context;

    public JobSeekerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<JobSeeker>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.JobSeekers
            .AsNoTracking()
            .OrderByDescending(j => j.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<JobSeeker?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.JobSeekers.FindAsync([id], cancellationToken);
    }

    public async Task<JobSeeker?> GetByIdWithUserAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.JobSeekers
            .Include(j => j.User)
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
    }

    public async Task<bool> UserExistsAsync(string email, string? phoneNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Users.AnyAsync(
            u => u.Email == email || (phoneNumber != null && u.PhoneNumber == phoneNumber),
            cancellationToken);
    }

    public async Task AddWithUserAsync(User user, JobSeeker jobSeeker, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        jobSeeker.UserId = user.Id;
        await _context.JobSeekers.AddAsync(jobSeeker, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(JobSeeker jobSeeker, User user, CancellationToken cancellationToken = default)
    {
        _context.JobSeekers.Update(jobSeeker);
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteWithUserAsync(JobSeeker jobSeeker, CancellationToken cancellationToken = default)
    {
        _context.JobSeekers.Remove(jobSeeker);

        var user = jobSeeker.User ?? await _context.Users.FindAsync([jobSeeker.UserId], cancellationToken);
        if (user != null)
        {
            _context.Users.Remove(user);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}