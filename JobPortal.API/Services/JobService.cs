using AutoMapper;
using JobPortal.API.DTOs;
using JobPortal.API.Models;
using JobPortal.API.Repositories;

namespace JobPortal.API.Services;

public class JobService : IJobService
{
    private readonly IJobRepository _repository;
    private readonly IMapper _mapper;

    public JobService(IJobRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<JobDto>> GetAllJobsAsync()
    {
        var list = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<JobDto>>(list);
    }

    public async Task<JobDto?> GetJobByIdAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return _mapper.Map<JobDto>(entity);
    }

    public async Task<JobDto> CreateJobAsync(JobDto jobDto)
    {
        var entity = _mapper.Map<Job>(jobDto);
        await _repository.AddAsync(entity);
        return _mapper.Map<JobDto>(entity);
    }

    public async Task UpdateJobAsync(long id, JobDto jobDto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return;
        _mapper.Map(jobDto, existing);
        existing.Id = id;
        await _repository.UpdateAsync(existing);
    }

    public async Task DeleteJobAsync(long id)
    {
        await _repository.DeleteAsync(id);
    }
}
