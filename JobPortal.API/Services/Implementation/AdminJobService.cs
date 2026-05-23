using JobPortal.API.Data;
using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;
using JobPortal.API.Exceptions;
using JobPortal.API.Helpers;
using JobPortal.API.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.API.Services.Implementation;

public class AdminJobService : IAdminJobService
{
    private const int MaxPageSize = 50;
    private readonly AppDbContext _context;
    private readonly IJobExpiryService _jobExpiryService;

    public AdminJobService(AppDbContext context, IJobExpiryService jobExpiryService)
    {
        _context = context;
        _jobExpiryService = jobExpiryService;
    }

    public async Task<PagedResult<AdminPendingJobDto>> GetPendingJobsPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1 || pageSize > MaxPageSize)
        {
            pageSize = 12;
        }

        await _jobExpiryService.CloseExpiredJobsAsync(cancellationToken);

        var query = _context.Jobs
            .AsNoTracking()
            .Where(j => j.PostingStatus == JobPostingCatalog.Pending);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(j =>
                j.Title.Contains(term) ||
                j.Description.Contains(term) ||
                j.Location.Contains(term) ||
                j.Employer.Name.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(j => j.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(j => new AdminPendingJobDto
            {
                Id = j.Id,
                EmployerId = j.EmployerId,
                EmployerName = j.Employer.Name,
                EmployerEmail = j.Employer.Email,
                CategoryId = j.CategoryId,
                CategoryName = j.Category.Name,
                Title = j.Title,
                Description = j.Description,
                Salary = j.Salary,
                Location = j.Location,
                PostingStatus = j.PostingStatus,
                WorkingHours = j.WorkingHours,
                ExpiryDate = j.ExpiryDate,
                ThumbnailUrl = j.Images.OrderBy(i => i.Id).Select(i => i.Url).FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<AdminPendingJobDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<JobDto> ApproveJobAsync(long jobId, CancellationToken cancellationToken = default)
    {
        await _jobExpiryService.CloseExpiredJobsAsync(cancellationToken);

        var job = await _context.Jobs
            .Include(j => j.Employer)
            .Include(j => j.Images.OrderBy(i => i.Id).Take(1))
            .FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken);

        if (job == null)
        {
            throw new NotFoundException($"Job with id {jobId} was not found.");
        }

        if (!string.Equals(job.PostingStatus, JobPostingCatalog.Pending, StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException("Chỉ có thể duyệt tin đang chờ phê duyệt.");
        }

        job.PostingStatus = JobPostingCatalog.Recruiting;
        await _context.SaveChangesAsync(cancellationToken);

        return MapJobDto(job);
    }

    public async Task<JobDto> RejectJobAsync(
        long jobId,
        RejectJobRequest? request,
        CancellationToken cancellationToken = default)
    {
        var job = await _context.Jobs
            .Include(j => j.Employer)
            .Include(j => j.Images.OrderBy(i => i.Id).Take(1))
            .FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken);

        if (job == null)
        {
            throw new NotFoundException($"Job with id {jobId} was not found.");
        }

        if (!string.Equals(job.PostingStatus, JobPostingCatalog.Pending, StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException("Chỉ có thể từ chối tin đang chờ phê duyệt.");
        }

        job.PostingStatus = JobPostingCatalog.Rejected;

        var employer = await _context.Employers
            .FirstOrDefaultAsync(e => e.Id == job.EmployerId, cancellationToken);

        if (employer != null)
        {
            employer.PostingLimit++;
            employer.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return MapJobDto(job);
    }

    private static JobDto MapJobDto(Models.Job job) => new()
    {
        Id = job.Id,
        EmployerId = job.EmployerId,
        EmployerName = job.Employer.Name,
        CategoryId = job.CategoryId,
        Title = job.Title,
        Description = job.Description,
        Salary = job.Salary,
        Location = job.Location,
        PostingStatus = job.PostingStatus,
        WorkingHours = job.WorkingHours,
        ExpiryDate = job.ExpiryDate,
        ThumbnailUrl = job.Images.OrderBy(i => i.Id).Select(i => i.Url).FirstOrDefault()
    };
}
