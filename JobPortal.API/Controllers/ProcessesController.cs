using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;
using JobPortal.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProcessesController : ControllerBase
{
    private readonly IProcessService _processService;

    public ProcessesController(IProcessService processService)
    {
        _processService = processService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProcesses(CancellationToken cancellationToken)
    {
        var items = await _processService.GetAllProcessesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ProcessDto>>.SuccessResponse(items, "Processes retrieved successfully."));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetProcess(long id, CancellationToken cancellationToken)
    {
        var item = await _processService.GetProcessByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<ProcessDto>.SuccessResponse(item, "Process retrieved successfully."));
    }

    [HttpPost]
    public async Task<IActionResult> CreateProcess([FromBody] ProcessDto dto, CancellationToken cancellationToken)
    {
        var created = await _processService.CreateProcessAsync(dto, cancellationToken);
        return CreatedAtAction(
            nameof(GetProcess),
            new { id = created.Id },
            ApiResponse<ProcessDto>.SuccessResponse(created, "Process created successfully."));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateProcess(long id, [FromBody] ProcessDto dto, CancellationToken cancellationToken)
    {
        await _processService.UpdateProcessAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Process updated successfully."));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteProcess(long id, CancellationToken cancellationToken)
    {
        await _processService.DeleteProcessAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Process deleted successfully."));
    }
}
