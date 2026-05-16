using JobPortal.API.Models;

namespace JobPortal.API.Repositories;

public interface IJobRepository
{
    Task<IEnumerable<Job>> GetAllAsync();
    Task<Job?> GetByIdAsync(long id);
    Task AddAsync(Job entity);
    Task UpdateAsync(Job entity);
    Task DeleteAsync(long id);
}
