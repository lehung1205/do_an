using JobPortal.API.DTOs;

namespace JobPortal.API.Services.Interface;

public interface IResumeService
{
    Task<IReadOnlyList<ResumeDto>> GetAllResumesAsync(CancellationToken cancellationToken = default);
    Task<ResumeDto> GetResumeByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<ResumeDto> CreateResumeAsync(ResumeDto dto, CancellationToken cancellationToken = default);
    Task UpdateResumeAsync(long id, ResumeDto dto, CancellationToken cancellationToken = default);
    Task DeleteResumeAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ResumeDto>> GetResumesForUserAsync(long userId, CancellationToken cancellationToken = default);
    Task<ResumeDto> CreateResumeForUserAsync(long userId, CreateResumeRequest request, CancellationToken cancellationToken = default);
    Task DeleteResumeForUserAsync(long userId, long resumeId, CancellationToken cancellationToken = default);
}
