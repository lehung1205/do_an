using AutoMapper;
using JobPortal.API.DTOs;
using JobPortal.API.Exceptions;
using JobPortal.API.Models;
using JobPortal.API.Repositories;

namespace JobPortal.API.Services;

public class WorkExperienceService : IWorkExperienceService
{
    private readonly IWorkExperienceRepository _repository;
    private readonly IMapper _mapper;

    public WorkExperienceService(IWorkExperienceRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<WorkExperienceDto>> GetAllWorkExperiencesAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<WorkExperienceDto>>(items);
    }

    public async Task<WorkExperienceDto> GetWorkExperienceByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            throw new NotFoundException($"Work experience with id {id} was not found.");
        }

        return _mapper.Map<WorkExperienceDto>(entity);
    }

    public async Task<WorkExperienceDto> CreateWorkExperienceAsync(WorkExperienceDto dto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<WorkExperience>(dto);
        await _repository.AddAsync(entity, cancellationToken);
        return _mapper.Map<WorkExperienceDto>(entity);
    }

    public async Task UpdateWorkExperienceAsync(long id, WorkExperienceDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            throw new NotFoundException($"Work experience with id {id} was not found.");
        }

        _mapper.Map(dto, existing);
        existing.Id = id;
        await _repository.UpdateAsync(existing, cancellationToken);
    }

    public async Task DeleteWorkExperienceAsync(long id, CancellationToken cancellationToken = default)
    {
        var deleted = await _repository.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            throw new NotFoundException($"Work experience with id {id} was not found.");
        }
    }
}
