using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;
using JobPortal.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet]
    public async Task<IActionResult> GetReviews(CancellationToken cancellationToken)
    {
        var items = await _reviewService.GetAllReviewsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ReviewDto>>.SuccessResponse(items, "Reviews retrieved successfully."));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetReview(long id, CancellationToken cancellationToken)
    {
        var item = await _reviewService.GetReviewByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<ReviewDto>.SuccessResponse(item, "Review retrieved successfully."));
    }

    [HttpPost]
    public async Task<IActionResult> CreateReview([FromBody] ReviewDto dto, CancellationToken cancellationToken)
    {
        var created = await _reviewService.CreateReviewAsync(dto, cancellationToken);
        return CreatedAtAction(
            nameof(GetReview),
            new { id = created.Id },
            ApiResponse<ReviewDto>.SuccessResponse(created, "Review created successfully."));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateReview(long id, [FromBody] ReviewDto dto, CancellationToken cancellationToken)
    {
        await _reviewService.UpdateReviewAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Review updated successfully."));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteReview(long id, CancellationToken cancellationToken)
    {
        await _reviewService.DeleteReviewAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Review deleted successfully."));
    }
}
