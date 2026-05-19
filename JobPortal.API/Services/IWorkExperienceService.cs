using JobPortal.API.DTOs;

namespace JobPortal.API.Services;

public interface IWorkExperienceService
{
    Task<IReadOnlyList<WorkExperienceDto>> GetAllWorkExperiencesAsync(CancellationToken cancellationToken = default);
    Task<WorkExperienceDto> GetWorkExperienceByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<WorkExperienceDto> CreateWorkExperienceAsync(WorkExperienceDto dto, CancellationToken cancellationToken = default);
    Task UpdateWorkExperienceAsync(long id, WorkExperienceDto dto, CancellationToken cancellationToken = default);
    Task DeleteWorkExperienceAsync(long id, CancellationToken cancellationToken = default);
}
