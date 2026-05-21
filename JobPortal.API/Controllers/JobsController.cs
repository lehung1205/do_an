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
        [FromQuery] string? q = null,
        [FromQuery] string? location = null,
        CancellationToken cancellationToken = default)
    {
        var pagedJobs = await _jobService.GetJobsPagedAsync(page, pageSize, q, location, cancellationToken);
        return Ok(ApiResponse<PagedResult<JobDto>>.SuccessResponse(pagedJobs, "Jobs retrieved successfully."));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetJob(long id, CancellationToken cancellationToken)
    {
        var job = await _jobService.GetJobByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<JobDto>.SuccessResponse(job, "Job retrieved successfully."));
    }

    [HttpPost]
    public async Task<IActionResult> CreateJob([FromBody] JobDto jobDto, CancellationToken cancellationToken)
    {
        var createdJob = await _jobService.CreateJobAsync(jobDto, cancellationToken);
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
