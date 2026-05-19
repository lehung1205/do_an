using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;
using JobPortal.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WorkExperiencesController : ControllerBase
{
    private readonly IWorkExperienceService _workExperienceService;

    public WorkExperiencesController(IWorkExperienceService workExperienceService)
    {
        _workExperienceService = workExperienceService;
    }

    [HttpGet]
    public async Task<IActionResult> GetWorkExperiences(CancellationToken cancellationToken)
    {
        var items = await _workExperienceService.GetAllWorkExperiencesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<WorkExperienceDto>>.SuccessResponse(items, "Work experiences retrieved successfully."));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetWorkExperience(long id, CancellationToken cancellationToken)
    {
        var item = await _workExperienceService.GetWorkExperienceByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<WorkExperienceDto>.SuccessResponse(item, "Work experience retrieved successfully."));
    }

    [HttpPost]
    public async Task<IActionResult> CreateWorkExperience([FromBody] WorkExperienceDto dto, CancellationToken cancellationToken)
    {
        var created = await _workExperienceService.CreateWorkExperienceAsync(dto, cancellationToken);
        return CreatedAtAction(
            nameof(GetWorkExperience),
            new { id = created.Id },
            ApiResponse<WorkExperienceDto>.SuccessResponse(created, "Work experience created successfully."));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateWorkExperience(long id, [FromBody] WorkExperienceDto dto, CancellationToken cancellationToken)
    {
        await _workExperienceService.UpdateWorkExperienceAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Work experience updated successfully."));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteWorkExperience(long id, CancellationToken cancellationToken)
    {
        await _workExperienceService.DeleteWorkExperienceAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Work experience deleted successfully."));
    }
}
