using JobPortal.API.DTOs;

namespace JobPortal.API.Services.Interface;

public interface IProcessService
{
    Task<IReadOnlyList<ProcessDto>> GetAllProcessesAsync(CancellationToken cancellationToken = default);
    Task<ProcessDto> GetProcessByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<ProcessDto> CreateProcessAsync(ProcessDto dto, CancellationToken cancellationToken = default);
    Task UpdateProcessAsync(long id, ProcessDto dto, CancellationToken cancellationToken = default);
    Task DeleteProcessAsync(long id, CancellationToken cancellationToken = default);
}
