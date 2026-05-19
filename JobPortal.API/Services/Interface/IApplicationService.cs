using JobPortal.API.DTOs;

namespace JobPortal.API.Services.Interface;

public interface IApplicationService
{
    Task<IReadOnlyList<ApplicationDto>> GetAllApplicationsAsync(CancellationToken cancellationToken = default);
    Task<ApplicationDto> GetApplicationByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<ApplicationDto> CreateApplicationAsync(ApplicationDto dto, CancellationToken cancellationToken = default);
    Task UpdateApplicationAsync(long id, ApplicationDto dto, CancellationToken cancellationToken = default);
    Task DeleteApplicationAsync(long id, CancellationToken cancellationToken = default);
}
