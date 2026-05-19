using JobPortal.API.DTOs;

namespace JobPortal.API.Services;

public interface IReviewService
{
    Task<IReadOnlyList<ReviewDto>> GetAllReviewsAsync(CancellationToken cancellationToken = default);
    Task<ReviewDto> GetReviewByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<ReviewDto> CreateReviewAsync(ReviewDto dto, CancellationToken cancellationToken = default);
    Task UpdateReviewAsync(long id, ReviewDto dto, CancellationToken cancellationToken = default);
    Task DeleteReviewAsync(long id, CancellationToken cancellationToken = default);
}
