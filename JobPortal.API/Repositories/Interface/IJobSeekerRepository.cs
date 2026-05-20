using JobPortal.API.Models;
using JobPortal.API.Models.Auth;

namespace JobPortal.API.Repositories.Interface;

public interface IJobSeekerRepository
{
    Task<IReadOnlyList<JobSeeker>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<JobSeeker?> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default);
    Task<JobSeeker?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<JobSeeker?> GetByIdWithUserAsync(long id, CancellationToken cancellationToken = default);
    Task<bool> UserExistsAsync(string email, string? phoneNumber, CancellationToken cancellationToken = default);
    Task AddWithUserAsync(User user, JobSeeker jobSeeker, CancellationToken cancellationToken = default);
    Task UpdateAsync(JobSeeker jobSeeker, User user, CancellationToken cancellationToken = default);
    Task DeleteWithUserAsync(JobSeeker jobSeeker, CancellationToken cancellationToken = default);
}
