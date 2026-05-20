using AutoMapper;
using JobPortal.API.DTOs;
using JobPortal.API.Exceptions;
using JobPortal.API.Models;
using JobPortal.API.Repositories.Interface;
using JobPortal.API.Services.Interface;


namespace JobPortal.API.Services.Implementation;

public class ResumeService : IResumeService
{
    private readonly IResumeRepository _repository;
    private readonly IJobSeekerRepository _jobSeekerRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IMapper _mapper;

    public ResumeService(
        IResumeRepository repository,
        IJobSeekerRepository jobSeekerRepository,
        IApplicationRepository applicationRepository,
        IMapper mapper)
    {
        _repository = repository;
        _jobSeekerRepository = jobSeekerRepository;
        _applicationRepository = applicationRepository;
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
        await _applicationRepository.DeleteByResumeIdAsync(id, cancellationToken);
        var deleted = await _repository.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            throw new NotFoundException($"Resume with id {id} was not found.");
        }
    }

    public async Task<IReadOnlyList<ResumeDto>> GetResumesForUserAsync(long userId, CancellationToken cancellationToken = default)
    {
        var jobSeeker = await _jobSeekerRepository.GetByUserIdAsync(userId, cancellationToken);
        if (jobSeeker == null)
        {
            return Array.Empty<ResumeDto>();
        }

        var items = await _repository.GetByJobSeekerIdAsync(jobSeeker.Id, cancellationToken);
        return _mapper.Map<IReadOnlyList<ResumeDto>>(items);
    }

    public async Task<ResumeDto> CreateResumeForUserAsync(long userId, CreateResumeRequest request, CancellationToken cancellationToken = default)
    {
        var jobSeeker = await _jobSeekerRepository.GetByUserIdAsync(userId, cancellationToken);
        if (jobSeeker == null)
        {
            throw new NotFoundException("Job seeker profile was not found for this account.");
        }

        var entity = new Resume
        {
            JobSeekerId = jobSeeker.Id,
            Title = request.Title.Trim(),
            Url = request.Url.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(entity, cancellationToken);
        return _mapper.Map<ResumeDto>(entity);
    }

    public async Task DeleteResumeForUserAsync(long userId, long resumeId, CancellationToken cancellationToken = default)
    {
        var jobSeeker = await _jobSeekerRepository.GetByUserIdAsync(userId, cancellationToken);
        if (jobSeeker == null)
        {
            throw new NotFoundException("Job seeker profile was not found for this account.");
        }

        var resume = await _repository.GetByIdAsync(resumeId, cancellationToken);
        if (resume == null)
        {
            throw new NotFoundException($"Resume with id {resumeId} was not found.");
        }

        if (resume.JobSeekerId != jobSeeker.Id)
        {
            throw new ForbiddenException("You cannot delete this resume.");
        }

        await DeleteResumeAsync(resumeId, cancellationToken);
    }
}
