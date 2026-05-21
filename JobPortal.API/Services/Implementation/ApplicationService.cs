using AutoMapper;
using JobPortal.API.Data;
using JobPortal.API.DTOs;
using JobPortal.API.Exceptions;
using JobPortal.API.Helpers;
using JobPortal.API.Models;
using JobPortal.API.Repositories.Interface;
using JobPortal.API.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.API.Services.Implementation;

public class ApplicationService : IApplicationService
{
    private const string DefaultStatus = "submitted";

    private readonly IApplicationRepository _repository;
    private readonly IJobSeekerRepository _jobSeekerRepository;
    private readonly IJobRepository _jobRepository;
    private readonly IResumeRepository _resumeRepository;
    private readonly IJobExpiryService _jobExpiryService;
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public ApplicationService(
        IApplicationRepository repository,
        IJobSeekerRepository jobSeekerRepository,
        IJobRepository jobRepository,
        IResumeRepository resumeRepository,
        IJobExpiryService jobExpiryService,
        AppDbContext context,
        IMapper mapper)
    {
        _repository = repository;
        _jobSeekerRepository = jobSeekerRepository;
        _jobRepository = jobRepository;
        _resumeRepository = resumeRepository;
        _jobExpiryService = jobExpiryService;
        _context = context;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ApplicationDto>> GetAllApplicationsAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<ApplicationDto>>(items);
    }

    public async Task<ApplicationDto> GetApplicationByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            throw new NotFoundException($"Application with id {id} was not found.");
        }

        return _mapper.Map<ApplicationDto>(entity);
    }

    public async Task<ApplicationDto> CreateApplicationAsync(ApplicationDto dto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<Application>(dto);
        if (entity.AppliedAt == default)
        {
            entity.AppliedAt = DateTime.UtcNow;
        }

        await _repository.AddAsync(entity, cancellationToken);
        return _mapper.Map<ApplicationDto>(entity);
    }

    public async Task UpdateApplicationAsync(long id, ApplicationDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing == null)
        {
            throw new NotFoundException($"Application with id {id} was not found.");
        }

        _mapper.Map(dto, existing);
        existing.Id = id;
        await _repository.UpdateAsync(existing, cancellationToken);
    }

    public async Task DeleteApplicationAsync(long id, CancellationToken cancellationToken = default)
    {
        var deleted = await _repository.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            throw new NotFoundException($"Application with id {id} was not found.");
        }
    }

    public async Task<MyApplicationDto> ApplyForJobAsync(
        long userId,
        CreateApplicationRequest request,
        CancellationToken cancellationToken = default)
    {
        await _jobExpiryService.CloseExpiredJobsAsync(cancellationToken);

        var jobSeeker = await _jobSeekerRepository.GetByUserIdAsync(userId, cancellationToken);
        if (jobSeeker == null)
        {
            throw new NotFoundException("Job seeker profile was not found.");
        }

        var job = await _jobRepository.GetByIdAsync(request.JobId, cancellationToken);
        if (job == null)
        {
            throw new NotFoundException($"Job with id {request.JobId} was not found.");
        }

        if (!string.Equals(job.PostingStatus, "recruiting", StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException(
                string.Equals(job.PostingStatus, "closed", StringComparison.OrdinalIgnoreCase)
                    ? "This job posting has closed and is no longer accepting applications."
                    : "This job is not accepting applications.");
        }

        var resume = await _resumeRepository.GetByIdAsync(request.ResumeId, cancellationToken);
        if (resume == null)
        {
            throw new NotFoundException($"Resume with id {request.ResumeId} was not found.");
        }

        if (resume.JobSeekerId != jobSeeker.Id)
        {
            throw new ForbiddenException("You can only apply with your own resume.");
        }

        if (await _repository.ExistsForJobSeekerAndJobAsync(jobSeeker.Id, request.JobId, cancellationToken))
        {
            throw new ConflictException("You have already applied to this job.");
        }

        var entity = new Application
        {
            JobSeekerId = jobSeeker.Id,
            JobId = request.JobId,
            ResumeId = request.ResumeId,
            AppliedAt = DateTime.UtcNow,
            Status = DefaultStatus
        };

        await _repository.AddAsync(entity, cancellationToken);

        return new MyApplicationDto
        {
            Id = entity.Id,
            JobId = job.Id,
            JobTitle = job.Title,
            JobLocation = job.Location,
            JobSalary = job.Salary,
            JobPostingStatus = job.PostingStatus,
            ResumeId = resume.Id,
            ResumeTitle = resume.Title,
            ResumeUrl = resume.Url,
            AppliedAt = entity.AppliedAt,
            Status = entity.Status
        };
    }

    public async Task<IReadOnlyList<MyApplicationDto>> GetMyApplicationsAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        var jobSeeker = await _jobSeekerRepository.GetByUserIdAsync(userId, cancellationToken);
        if (jobSeeker == null)
        {
            throw new NotFoundException("Job seeker profile was not found.");
        }

        var items = await _repository.GetByJobSeekerIdAsync(jobSeeker.Id, cancellationToken);
        return items.Select(MapToMyApplicationDto).ToList();
    }

    public async Task<bool> HasAppliedToJobAsync(
        long userId,
        long jobId,
        CancellationToken cancellationToken = default)
    {
        var jobSeeker = await _jobSeekerRepository.GetByUserIdAsync(userId, cancellationToken);
        if (jobSeeker == null)
        {
            return false;
        }

        return await _repository.ExistsForJobSeekerAndJobAsync(jobSeeker.Id, jobId, cancellationToken);
    }

    public async Task<IReadOnlyList<SeekerWorkProgressListItemDto>> GetMyAcceptedWorkProgressListAsync(
        long userId,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var jobSeekerId = await GetJobSeekerIdForUserAsync(userId, cancellationToken);

        var query = _context.Applications
            .AsNoTracking()
            .Where(a => a.JobSeekerId == jobSeekerId && a.Status == "accepted");

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(a =>
                a.Job.Title.Contains(term) ||
                a.Job.Employer.Name.Contains(term) ||
                a.Job.Location.Contains(term));
        }

        var applications = await query
            .OrderByDescending(a => a.AppliedAt)
            .Select(a => new
            {
                a.Id,
                a.JobId,
                a.AppliedAt,
                JobTitle = a.Job.Title,
                CompanyName = a.Job.Employer.Name,
                JobLocation = a.Job.Location,
                Steps = a.Processes
                    .OrderByDescending(p => p.CreatedAt)
                    .ThenByDescending(p => p.Id)
                    .Select(p => new { p.Status, p.Title, p.CreatedAt })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return applications.Select(a =>
        {
            var latest = a.Steps.FirstOrDefault();
            return new SeekerWorkProgressListItemDto
            {
                ApplicationId = a.Id,
                JobId = a.JobId,
                JobTitle = a.JobTitle,
                CompanyName = a.CompanyName,
                JobLocation = a.JobLocation,
                AppliedAt = a.AppliedAt,
                CurrentWorkStatus = latest?.Status,
                CurrentWorkTitle = latest?.Title,
                LastProgressAt = latest?.CreatedAt,
                StepCount = a.Steps.Count,
                IsProgressLocked = latest != null && WorkProgressCatalog.IsLockedStatus(latest.Status)
            };
        }).ToList();
    }

    public async Task<SeekerApplicationWorkProgressDto> GetMyApplicationWorkProgressAsync(
        long userId,
        long applicationId,
        CancellationToken cancellationToken = default)
    {
        var jobSeekerId = await GetJobSeekerIdForUserAsync(userId, cancellationToken);
        var application = await GetAcceptedApplicationForSeekerAsync(jobSeekerId, applicationId, cancellationToken);

        var steps = await _context.Processes
            .AsNoTracking()
            .Where(p => p.ApplicationId == applicationId)
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .ToListAsync(cancellationToken);

        var stepDtos = steps.Select(MapWorkProgressStep).ToList();
        var currentStep = stepDtos.FirstOrDefault();

        return new SeekerApplicationWorkProgressDto
        {
            ApplicationId = application.Id,
            JobId = application.JobId,
            JobTitle = application.Job.Title,
            CompanyName = application.Job.Employer.Name,
            JobLocation = application.Job.Location,
            JobSalary = application.Job.Salary,
            AppliedAt = application.AppliedAt,
            ApplicationStatus = application.Status,
            Steps = stepDtos,
            CurrentStep = currentStep,
            IsProgressLocked = WorkProgressCatalog.IsLockedStatus(currentStep?.Status)
        };
    }

    private async Task<long> GetJobSeekerIdForUserAsync(long userId, CancellationToken cancellationToken)
    {
        var jobSeeker = await _jobSeekerRepository.GetByUserIdAsync(userId, cancellationToken);
        if (jobSeeker == null)
        {
            throw new NotFoundException("Job seeker profile was not found.");
        }

        return jobSeeker.Id;
    }

    private async Task<Application> GetAcceptedApplicationForSeekerAsync(
        long jobSeekerId,
        long applicationId,
        CancellationToken cancellationToken)
    {
        var application = await _context.Applications
            .AsNoTracking()
            .Include(a => a.Job)
            .ThenInclude(j => j.Employer)
            .FirstOrDefaultAsync(
                a => a.Id == applicationId && a.JobSeekerId == jobSeekerId,
                cancellationToken);

        if (application == null)
        {
            throw new NotFoundException($"Application with id {applicationId} was not found.");
        }

        if (!string.Equals(application.Status, "accepted", StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException("Chỉ xem tiến độ cho đơn ứng tuyển đã được chấp nhận.");
        }

        return application;
    }

    private static WorkProgressStepDto MapWorkProgressStep(Process p) => new()
    {
        Id = p.Id,
        ApplicationId = p.ApplicationId,
        Status = p.Status,
        Title = p.Title,
        Notes = p.Notes,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };

    private static MyApplicationDto MapToMyApplicationDto(Application a) =>
        new()
        {
            Id = a.Id,
            JobId = a.JobId,
            JobTitle = a.Job.Title,
            JobLocation = a.Job.Location,
            JobSalary = a.Job.Salary,
            JobPostingStatus = a.Job.PostingStatus,
            ResumeId = a.ResumeId,
            ResumeTitle = a.Resume.Title,
            ResumeUrl = a.Resume.Url,
            AppliedAt = a.AppliedAt,
            Status = a.Status
        };
}
