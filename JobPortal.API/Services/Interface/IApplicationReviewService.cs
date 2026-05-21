using JobPortal.API.DTOs;

namespace JobPortal.API.Services.Interface;

public interface IApplicationReviewService
{
    Task<ApplicationReviewContextDto> GetEmployerReviewContextAsync(
        long userId,
        long applicationId,
        CancellationToken cancellationToken = default);

    Task<ApplicationReviewViewDto> SubmitEmployerReviewAsync(
        long userId,
        long applicationId,
        CreateApplicationReviewRequest request,
        CancellationToken cancellationToken = default);

    Task<ApplicationReviewContextDto> GetSeekerReviewContextAsync(
        long userId,
        long applicationId,
        CancellationToken cancellationToken = default);

    Task<ApplicationReviewViewDto> SubmitSeekerReviewAsync(
        long userId,
        long applicationId,
        CreateApplicationReviewRequest request,
        CancellationToken cancellationToken = default);

    Task<SeekerReceivedReviewsSummaryDto> GetSeekerReceivedReviewsAsync(
        long userId,
        CancellationToken cancellationToken = default);

    Task<EmployerReceivedReviewsSummaryDto> GetEmployerReceivedReviewsAsync(
        long userId,
        CancellationToken cancellationToken = default);
}
