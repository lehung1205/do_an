namespace JobPortal.Web.Models;

public class RatingStarsModel
{
    public double? AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public string EmptyMessage { get; set; } = "Chưa có đánh giá";

    public static RatingStarsModel FromEmployer(double? average, int count) =>
        new()
        {
            AverageRating = average,
            ReviewCount = count,
            EmptyMessage = "Chưa có đánh giá"
        };
}
