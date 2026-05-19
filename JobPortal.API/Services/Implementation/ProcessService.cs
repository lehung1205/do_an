using AutoMapper;
using JobPortal.API.DTOs;
using JobPortal.API.Exceptions;
using JobPortal.API.Models;
using JobPortal.API.Repositories.Interface;
using JobPortal.API.Services.Interface;


namespace JobPortal.API.Services.Implementation;

public class ProcessService : IProcessService
{
    private readonly IProcessRepository _repository;
    private readonly IMapper _mapper;

    public ProcessService(IProcessRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ProcessDto>> GetAllProcessesAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<ProcessDto>>(items);
    }

    public async Task<ProcessDto> GetProcessByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            throw new NotFoundException($"Process with id {id} was not found.");
        }

        return _mapper.Map<ProcessDto>(entity);
    }

    public async Task<ProcessDto> CreateProcessAsync(ProcessDto dto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<Process>(dto);
        await _repository.AddAsync(entity, cancellationToken);
        return _mapper.Map<ProcessDto>(entity);
    }

    public async Task UpdateProcessAsync(long id, ProcessDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            throw new NotFoundException($"Process with id {id} was not found.");
        }

        _mapper.Map(dto, existing);
        existing.Id = id;
        await _repository.UpdateAsync(existing, cancellationToken);
    }

    public async Task DeleteProcessAsync(long id, CancellationToken cancellationToken = default)
    {
        var deleted = await _repository.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            throw new NotFoundException($"Process with id {id} was not found.");
        }
    }
}
