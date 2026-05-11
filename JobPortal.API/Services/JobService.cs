using AutoMapper;
using JobPortal.API.DTOs;
using JobPortal.API.Models;
using JobPortal.API.Repositories;

namespace JobPortal.API.Services;

public class JobService : IJobService
{
    private readonly ICongViecRepository _repository;
    private readonly IMapper _mapper;

    public JobService(ICongViecRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CongViecDto>> GetAllJobsAsync()
    {
        var list = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<CongViecDto>>(list);
    }

    public async Task<CongViecDto?> GetJobByIdAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return _mapper.Map<CongViecDto>(entity);
    }

    public async Task<CongViecDto> CreateJobAsync(CongViecDto jobDto)
    {
        var entity = _mapper.Map<CongViec>(jobDto);
        await _repository.AddAsync(entity);
        return _mapper.Map<CongViecDto>(entity);
    }

    public async Task UpdateJobAsync(long id, CongViecDto jobDto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return;
        _mapper.Map(jobDto, existing);
        existing.IdCongViec = id;
        await _repository.UpdateAsync(existing);
    }

    public async Task DeleteJobAsync(long id)
    {
        await _repository.DeleteAsync(id);
    }
}
