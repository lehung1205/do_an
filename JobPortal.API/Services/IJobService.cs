using JobPortal.API.DTOs;

namespace JobPortal.API.Services;

public interface IJobService
{
    Task<IEnumerable<JobDto>> GetAllJobsAsync();
    Task<JobDto?> GetJobByIdAsync(long id);
    Task<JobDto> CreateJobAsync(JobDto jobDto);
    Task UpdateJobAsync(long id, JobDto jobDto);
    Task DeleteJobAsync(long id);
}
