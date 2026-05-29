using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;
using JobPortal.API.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StatsController : ControllerBase
{
    private readonly IStatsService _statsService;

    public StatsController(IStatsService statsService) => _statsService = statsService;

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetHomeStats(CancellationToken cancellationToken)
    {
        var stats = await _statsService.GetHomeStatsAsync(cancellationToken);
        return Ok(ApiResponse<HomeStatsDto>.SuccessResponse(stats, "Home stats retrieved successfully."));
    }

    [AllowAnonymous]
    [HttpGet("top-rated")]
    public async Task<IActionResult> GetTopRatedLists(CancellationToken cancellationToken)
    {
        var lists = await _statsService.GetTopRatedListsAsync(cancellationToken);
        return Ok(ApiResponse<TopRatedListsDto>.SuccessResponse(lists, "Top rated lists retrieved successfully."));
    }
}
