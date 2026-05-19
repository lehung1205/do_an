using AutoMapper;
using JobPortal.API.DTOs;
using JobPortal.API.Exceptions;
using JobPortal.API.Models;
using JobPortal.API.Repositories;

namespace JobPortal.API.Services;

public class ResumeService : IResumeService
{
    private readonly IResumeRepository _repository;
    private readonly IMapper _mapper;

    public ResumeService(IResumeRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ResumeDto>> GetAllResumesAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<ResumeDto>>(items);
    }

    public async Task<ResumeDto> GetResumeByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            throw new NotFoundException($"Resume with id {id} was not found.");
        }

        return _mapper.Map<ResumeDto>(entity);
    }

    public async Task<ResumeDto> CreateResumeAsync(ResumeDto dto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<Resume>(dto);
        if (entity.CreatedAt == default)
        {
            entity.CreatedAt = DateTime.UtcNow;
        }

        await _repository.AddAsync(entity, cancellationToken);
        return _mapper.Map<ResumeDto>(entity);
    }

    public async Task UpdateResumeAsync(long id, ResumeDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            throw new NotFoundException($"Resume with id {id} was not found.");
        }

        _mapper.Map(dto, existing);
        existing.Id = id;
        await _repository.UpdateAsync(existing, cancellationToken);
    }

    public async Task DeleteResumeAsync(long id, CancellationToken cancellationToken = default)
    {
        var deleted = await _repository.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            throw new NotFoundException($"Resume with id {id} was not found.");
        }
    }
}
