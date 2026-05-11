using JobPortal.API.Models;

namespace JobPortal.API.Repositories;

public interface ICongViecRepository
{
    Task<IEnumerable<CongViec>> GetAllAsync();
    Task<CongViec?> GetByIdAsync(long id);
    Task AddAsync(CongViec entity);
    Task UpdateAsync(CongViec entity);
    Task DeleteAsync(long id);
}
