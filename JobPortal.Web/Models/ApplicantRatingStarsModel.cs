namespace JobPortal.Web.Models;

public class ApplicantRatingStarsModel
{
    public double? AverageRating { get; set; }
    public int ReviewCount { get; set; }

    public static ApplicantRatingStarsModel FromApplication(JobPortal.Web.Dtos.EmployerDashboardApplicationDto app) =>
        new()
        {
            AverageRating = app.ApplicantAverageRating,
            ReviewCount = app.ApplicantReviewCount
        };

    public RatingStarsModel ToRatingStarsModel() =>
        new()
        {
            AverageRating = AverageRating,
            ReviewCount = ReviewCount,
            EmptyMessage = "Chưa có đánh giá"
        };
}
