using AutoMapper;
using JobPortal.API.Data;
using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;
using JobPortal.API.Exceptions;
using JobPortal.API.Helpers;
using JobPortal.API.Models;
using JobPortal.API.Repositories.Interface;
using JobPortal.API.Services.Interface;
using Microsoft.EntityFrameworkCore;


namespace JobPortal.API.Services.Implementation;

public class JobService : IJobService
{
    private const int MaxPageSize = 100;
    private readonly IJobRepository _repository;
    private readonly IEmployerRepository _employerRepository;
    private readonly IJobExpiryService _jobExpiryService;
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public JobService(
        IJobRepository repository,
        IEmployerRepository employerRepository,
        IJobExpiryService jobExpiryService,
        AppDbContext context,
        IMapper mapper)
    {
        _repository = repository;
        _employerRepository = employerRepository;
        _jobExpiryService = jobExpiryService;
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<JobDto>> GetJobsPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        string? location = null,
        CancellationToken cancellationToken = default)
    {
        ValidatePagination(page, pageSize);

        await _jobExpiryService.CloseExpiredJobsAsync(cancellationToken);

        var (items, totalCount) = await _repository.GetPagedAsync(
            page,
            pageSize,
            recruitingOnly: true,
            search,
            location,
            cancellationToken);
        var dtos = _mapper.Map<List<JobDto>>(items);
        await ApplyEmployerRatingsAsync(dtos, cancellationToken);

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
        await _jobExpiryService.CloseExpiredJobsAsync(cancellationToken);

        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            throw new NotFoundException($"Job with id {id} was not found.");
        }

        if (!JobPostingCatalog.IsPubliclyVisible(entity.PostingStatus))
        {
            throw new NotFoundException($"Job with id {id} was not found.");
        }

        var dto = _mapper.Map<JobDto>(entity);
        await ApplyEmployerRatingsAsync(new List<JobDto> { dto }, cancellationToken);
        return dto;
    }

    public async Task<JobDto> CreateJobAsync(CreateJobRequest request, CancellationToken cancellationToken = default)
    {
        var employer = await _employerRepository.GetByIdAsync(request.EmployerId, cancellationToken);
        if (employer == null)
        {
            throw new NotFoundException($"Employer with id {request.EmployerId} was not found.");
        }

        if (employer.PostingLimit < 1)
        {
            throw new BadRequestException("Bạn đã hết lượt đăng tin. Vui lòng mua gói để thêm lượt.");
        }

        employer.PostingLimit--;
        employer.UpdatedAt = DateTime.UtcNow;

        var entity = _mapper.Map<Job>(request);
        entity.PostingStatus = JobPostingCatalog.Pending;
        entity.CreatedAt = DateTime.UtcNow;
        await _repository.AddAsync(entity, cancellationToken);

        var created = await _repository.GetByIdAsync(entity.Id, cancellationToken)
            ?? entity;
        var dto = _mapper.Map<JobDto>(created);
        await ApplyEmployerRatingsAsync(new List<JobDto> { dto }, cancellationToken);
        return dto;
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

    private async Task ApplyEmployerRatingsAsync(
        IList<JobDto> jobs,
        CancellationToken cancellationToken)
    {
        if (jobs.Count == 0)
        {
            return;
        }

        var employerIds = jobs.Select(j => j.EmployerId).Distinct().ToList();
        var ratings = await LoadEmployerRatingsAsync(employerIds, cancellationToken);

        foreach (var job in jobs)
        {
            if (ratings.TryGetValue(job.EmployerId, out var rating))
            {
                job.EmployerAverageRating = rating.Average;
                job.EmployerReviewCount = rating.Count;
            }
        }
    }

    private async Task<Dictionary<long, EmployerRatingSnapshot>> LoadEmployerRatingsAsync(
        IEnumerable<long> employerIds,
        CancellationToken cancellationToken)
    {
        var ids = employerIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return new Dictionary<long, EmployerRatingSnapshot>();
        }

        var rows = await _context.Reviews
            .AsNoTracking()
            .Where(r =>
                r.ReviewType == ReviewCatalog.SeekerToEmployer &&
                ids.Contains(r.EmployerId))
            .GroupBy(r => r.EmployerId)
            .Select(g => new
            {
                EmployerId = g.Key,
                Average = g.Average(x => (double)x.Rating),
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(
            x => x.EmployerId,
            x => new EmployerRatingSnapshot
            {
                Average = Math.Round(x.Average, 1),
                Count = x.Count
            });
    }

    private sealed class EmployerRatingSnapshot
    {
        public double Average { get; init; }
        public int Count { get; init; }
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
