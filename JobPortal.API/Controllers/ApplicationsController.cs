using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;
using JobPortal.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _applicationService;

    public ApplicationsController(IApplicationService applicationService)
    {
        _applicationService = applicationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetApplications(CancellationToken cancellationToken)
    {
        var items = await _applicationService.GetAllApplicationsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ApplicationDto>>.SuccessResponse(items, "Applications retrieved successfully."));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetApplication(long id, CancellationToken cancellationToken)
    {
        var item = await _applicationService.GetApplicationByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<ApplicationDto>.SuccessResponse(item, "Application retrieved successfully."));
    }

    [HttpPost]
    public async Task<IActionResult> CreateApplication([FromBody] ApplicationDto dto, CancellationToken cancellationToken)
    {
        var created = await _applicationService.CreateApplicationAsync(dto, cancellationToken);
        return CreatedAtAction(
            nameof(GetApplication),
            new { id = created.Id },
            ApiResponse<ApplicationDto>.SuccessResponse(created, "Application created successfully."));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateApplication(long id, [FromBody] ApplicationDto dto, CancellationToken cancellationToken)
    {
        await _applicationService.UpdateApplicationAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Application updated successfully."));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteApplication(long id, CancellationToken cancellationToken)
    {
        await _applicationService.DeleteApplicationAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Application deleted successfully."));
    }
}
