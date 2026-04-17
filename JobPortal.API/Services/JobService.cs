using AutoMapper;
using JobPortal.API.DTOs;
using JobPortal.API.Models;
using JobPortal.API.Repositories;

namespace JobPortal.API.Services;

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;
    private readonly IMapper _mapper;

    public JobService(IJobRepository jobRepository, IMapper mapper)
    {
        _jobRepository = jobRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<JobDto>> GetAllJobsAsync()
    {
        var jobs = await _jobRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<JobDto>>(jobs);
    }

    public async Task<JobDto?> GetJobByIdAsync(int id)
    {
        var job = await _jobRepository.GetByIdAsync(id);
        return _mapper.Map<JobDto>(job);
    }

    public async Task<JobDto> CreateJobAsync(JobDto jobDto)
    {
        var job = _mapper.Map<Job>(jobDto);
        await _jobRepository.AddAsync(job);
        return _mapper.Map<JobDto>(job);
    }

    public async Task UpdateJobAsync(int id, JobDto jobDto)
    {
        var existingJob = await _jobRepository.GetByIdAsync(id);
        if (existingJob != null)
        {
            _mapper.Map(jobDto, existingJob);
            existingJob.Id = id; // Ensure ID doesn't change
            await _jobRepository.UpdateAsync(existingJob);
        }
    }

    public async Task DeleteJobAsync(int id)
    {
        await _jobRepository.DeleteAsync(id);
    }
}
