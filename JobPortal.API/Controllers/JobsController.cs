using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;
using JobPortal.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;

    public JobsController(IJobService jobService)
    {
        _jobService = jobService;
    }

    [HttpGet]
    public async Task<IActionResult> GetJobs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] int? limit = null,
        [FromQuery] string? q = null,
        [FromQuery] string? location = null,
        [FromQuery] long? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var effectivePageSize = limit is > 0 ? limit.Value : pageSize;
        var excludeAppliedForUserId = TryGetCurrentJobSeekerUserId();
        var pagedJobs = await _jobService.GetJobsPagedAsync(
            page,
            effectivePageSize,
            q,
            location,
            categoryId,
            excludeAppliedForUserId,
            cancellationToken);
        return Ok(ApiResponse<PagedResult<JobListItemDto>>.SuccessResponse(pagedJobs, "Jobs retrieved successfully."));
    }

    private long? TryGetCurrentJobSeekerUserId()
    {
        if (User.Identity?.IsAuthenticated != true || !User.IsInRole("JOB_SEEKER"))
        {
            return null;
        }

        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return long.TryParse(sub, out var userId) ? userId : null;
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetJob(long id, CancellationToken cancellationToken)
    {
        var job = await _jobService.GetJobByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<JobDto>.SuccessResponse(job, "Job retrieved successfully."));
    }

    [HttpGet("{id:long}/related")]
    public async Task<IActionResult> GetRelatedJobs(long id, CancellationToken cancellationToken)
    {
        var related = await _jobService.GetRelatedJobsAsync(id, cancellationToken);
        return Ok(ApiResponse<JobRelatedListsDto>.SuccessResponse(related, "Related jobs retrieved successfully."));
    }

    [HttpPost]
    public async Task<IActionResult> CreateJob([FromBody] CreateJobRequest request, CancellationToken cancellationToken)
    {
        var createdJob = await _jobService.CreateJobAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetJob),
            new { id = createdJob.Id },
            ApiResponse<JobDto>.SuccessResponse(createdJob, "Job created successfully."));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateJob(long id, [FromBody] JobDto jobDto, CancellationToken cancellationToken)
    {
        await _jobService.UpdateJobAsync(id, jobDto, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Job updated successfully."));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteJob(long id, CancellationToken cancellationToken)
    {
        await _jobService.DeleteJobAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Job deleted successfully."));
    }
}
