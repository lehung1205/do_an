using JobPortal.API.DTOs;

namespace JobPortal.API.Services;

public interface IJobSeekerService
{
    Task<IReadOnlyList<JobSeekerDto>> GetAllJobSeekersAsync(CancellationToken cancellationToken = default);
    Task<JobSeekerDto> GetJobSeekerByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<JobSeekerDto> CreateJobSeekerAsync(CreateJobSeekerDto dto, CancellationToken cancellationToken = default);
    Task UpdateJobSeekerAsync(long id, UpdateJobSeekerDto dto, CancellationToken cancellationToken = default);
    Task DeleteJobSeekerAsync(long id, CancellationToken cancellationToken = default);
}
