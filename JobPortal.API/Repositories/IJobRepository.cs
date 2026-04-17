using JobPortal.API.Models;

namespace JobPortal.API.Repositories;

public interface IJobRepository
{
    Task<IEnumerable<Job>> GetAllAsync();
    Task<Job?> GetByIdAsync(int id);
    Task AddAsync(Job job);
    Task UpdateAsync(Job job);
    Task DeleteAsync(int id);
}
