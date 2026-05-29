using JobPortal.API.DTOs;

namespace JobPortal.API.Services.Interface;

public interface IApplicationService
{
    Task<IReadOnlyList<ApplicationDto>> GetAllApplicationsAsync(CancellationToken cancellationToken = default);
    Task<ApplicationDto> GetApplicationByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<ApplicationDto> CreateApplicationAsync(ApplicationDto dto, CancellationToken cancellationToken = default);
    Task UpdateApplicationAsync(long id, ApplicationDto dto, CancellationToken cancellationToken = default);
    Task DeleteApplicationAsync(long id, CancellationToken cancellationToken = default);

    Task<MyApplicationDto> ApplyForJobAsync(long userId, CreateApplicationRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MyApplicationDto>> GetMyApplicationsAsync(long userId, CancellationToken cancellationToken = default);

    Task<bool> HasAppliedToJobAsync(long userId, long jobId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SeekerWorkProgressListItemDto>> GetMyAcceptedWorkProgressListAsync(
        long userId,
        string? search = null,
        string? progress = null,
        CancellationToken cancellationToken = default);

    Task<SeekerApplicationWorkProgressDto> GetMyApplicationWorkProgressAsync(
        long userId,
        long applicationId,
        CancellationToken cancellationToken = default);
}
