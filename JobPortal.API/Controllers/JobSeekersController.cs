using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;
using JobPortal.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class JobSeekersController : ControllerBase
{
    private readonly IJobSeekerService _jobSeekerService;

    public JobSeekersController(IJobSeekerService jobSeekerService)
    {
        _jobSeekerService = jobSeekerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetJobSeekers(CancellationToken cancellationToken)
    {
        var items = await _jobSeekerService.GetAllJobSeekersAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<JobSeekerDto>>.SuccessResponse(items, "Job seekers retrieved successfully."));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetJobSeeker(long id, CancellationToken cancellationToken)
    {
        var item = await _jobSeekerService.GetJobSeekerByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<JobSeekerDto>.SuccessResponse(item, "Job seeker retrieved successfully."));
    }

    [HttpPost]
    public async Task<IActionResult> CreateJobSeeker([FromBody] CreateJobSeekerDto dto, CancellationToken cancellationToken)
    {
        var created = await _jobSeekerService.CreateJobSeekerAsync(dto, cancellationToken);
        return CreatedAtAction(
            nameof(GetJobSeeker),
            new { id = created.Id },
            ApiResponse<JobSeekerDto>.SuccessResponse(created, "Job seeker created successfully."));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateJobSeeker(long id, [FromBody] UpdateJobSeekerDto dto, CancellationToken cancellationToken)
    {
        await _jobSeekerService.UpdateJobSeekerAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Job seeker updated successfully."));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteJobSeeker(long id, CancellationToken cancellationToken)
    {
        await _jobSeekerService.DeleteJobSeekerAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Job seeker deleted successfully."));
    }
}
