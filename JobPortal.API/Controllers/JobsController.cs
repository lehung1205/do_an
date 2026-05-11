using JobPortal.API.DTOs;
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
    public async Task<ActionResult<IEnumerable<CongViecDto>>> GetJobs()
    {
        var jobs = await _jobService.GetAllJobsAsync();
        return Ok(jobs);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<CongViecDto>> GetJob(long id)
    {
        var job = await _jobService.GetJobByIdAsync(id);
        if (job == null) return NotFound();
        return Ok(job);
    }

    [HttpPost]
    public async Task<ActionResult<CongViecDto>> CreateJob(CongViecDto jobDto)
    {
        var createdJob = await _jobService.CreateJobAsync(jobDto);
        return CreatedAtAction(nameof(GetJob), new { id = createdJob.id_cong_viec }, createdJob);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateJob(long id, CongViecDto jobDto)
    {
        await _jobService.UpdateJobAsync(id, jobDto);
        return NoContent();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteJob(long id)
    {
        await _jobService.DeleteJobAsync(id);
        return NoContent();
    }
}
