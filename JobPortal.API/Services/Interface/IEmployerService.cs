using JobPortal.API.DTOs;

namespace JobPortal.API.Services.Interface;

public interface IEmployerService
{
    Task<IReadOnlyList<EmployerDto>> GetAllEmployersAsync(CancellationToken cancellationToken = default);
    Task<EmployerDto> GetEmployerByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<EmployerPublicProfileDto> GetEmployerPublicProfileAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmployerWithRatingDto>> GetAllEmployersWithRatingAsync(CancellationToken cancellationToken = default);
    Task<EmployerDto> CreateEmployerAsync(CreateEmployerDto dto, CancellationToken cancellationToken = default);
    Task UpdateEmployerAsync(long id, UpdateEmployerDto dto, CancellationToken cancellationToken = default);
    Task DeleteEmployerAsync(long id, CancellationToken cancellationToken = default);
}
