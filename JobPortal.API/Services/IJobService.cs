using JobPortal.API.DTOs;

namespace JobPortal.API.Services;

public interface IJobService
{
    Task<IEnumerable<JobDto>> GetAllJobsAsync();
    Task<JobDto?> GetJobByIdAsync(int id);
    Task<JobDto> CreateJobAsync(JobDto jobDto);
    Task UpdateJobAsync(int id, JobDto jobDto);
    Task DeleteJobAsync(int id);
}
