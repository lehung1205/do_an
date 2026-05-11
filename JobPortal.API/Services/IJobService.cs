using JobPortal.API.DTOs;

namespace JobPortal.API.Services;

public interface IJobService
{
    Task<IEnumerable<CongViecDto>> GetAllJobsAsync();
    Task<CongViecDto?> GetJobByIdAsync(long id);
    Task<CongViecDto> CreateJobAsync(CongViecDto jobDto);
    Task UpdateJobAsync(long id, CongViecDto jobDto);
    Task DeleteJobAsync(long id);
}
