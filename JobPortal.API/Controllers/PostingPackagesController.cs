using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;
using JobPortal.API.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PostingPackagesController : ControllerBase
{
    private readonly IPostingPackageService _postingPackageService;

    public PostingPackagesController(IPostingPackageService postingPackageService)
    {
        _postingPackageService = postingPackageService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPostingPackages(CancellationToken cancellationToken)
    {
        var packages = await _postingPackageService.GetAllPostingPackagesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PostingPackageDto>>.SuccessResponse(
            packages,
            "Posting packages retrieved successfully."));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetPostingPackage(long id, CancellationToken cancellationToken)
    {
        var package = await _postingPackageService.GetPostingPackageByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<PostingPackageDto>.SuccessResponse(package, "Posting package retrieved successfully."));
    }
}
