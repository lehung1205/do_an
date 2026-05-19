using AutoMapper;
using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;
using JobPortal.API.Exceptions;
using JobPortal.API.Models;
using JobPortal.API.Repositories;

namespace JobPortal.API.Services;

public class JobService : IJobService
{
    private const int MaxPageSize = 100;
    private readonly IJobRepository _repository;
    private readonly IMapper _mapper;

    public JobService(IJobRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<JobDto>> GetJobsPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        ValidatePagination(page, pageSize);

        var (items, totalCount) = await _repository.GetPagedAsync(page, pageSize, cancellationToken);
        var dtos = _mapper.Map<IReadOnlyList<JobDto>>(items);

        return new PagedResult<JobDto>
        {
            Items = dtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<JobDto> GetJobByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            throw new NotFoundException($"Job with id {id} was not found.");
        }

        return _mapper.Map<JobDto>(entity);
    }

    public async Task<JobDto> CreateJobAsync(JobDto jobDto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<Job>(jobDto);
        await _repository.AddAsync(entity, cancellationToken);
        return _mapper.Map<JobDto>(entity);
    }

    public async Task UpdateJobAsync(long id, JobDto jobDto, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            throw new NotFoundException($"Job with id {id} was not found.");
        }

        _mapper.Map(jobDto, existing);
        existing.Id = id;
        await _repository.UpdateAsync(existing, cancellationToken);
    }

    public async Task DeleteJobAsync(long id, CancellationToken cancellationToken = default)
    {
        var deleted = await _repository.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            throw new NotFoundException($"Job with id {id} was not found.");
        }
    }

    private static void ValidatePagination(int page, int pageSize)
    {
        if (page < 1)
        {
            throw new BadRequestException("Page must be at least 1.");
        }

        if (pageSize < 1 || pageSize > MaxPageSize)
        {
            throw new BadRequestException($"Page size must be between 1 and {MaxPageSize}.");
        }
    }
}
