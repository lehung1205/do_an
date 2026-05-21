namespace JobPortal.Web.Models;

public class ReceivedReviewsPanelModel
{
    public double? AverageRating { get; set; }
    public int TotalCount { get; set; }
    public string AverageLabel { get; set; } = "";
    public string EmptyMessage { get; set; } = "";
    public IReadOnlyList<ReceivedReviewItemPanelModel> Items { get; set; } = Array.Empty<ReceivedReviewItemPanelModel>();

    public static ReceivedReviewsPanelModel FromSeeker(Dtos.SeekerReceivedReviewsSummaryDto summary) =>
        new()
        {
            AverageRating = summary.AverageRating,
            TotalCount = summary.TotalCount,
            AverageLabel = "Điểm trung bình từ nhà tuyển dụng",
            EmptyMessage = "Chưa có đánh giá nào từ nhà tuyển dụng.",
            Items = summary.Items.Select(i => new ReceivedReviewItemPanelModel
            {
                ApplicationId = i.ApplicationId,
                Rating = i.Rating,
                Comment = i.Comment,
                PrimaryLabel = i.EmployerName,
                SecondaryLabel = i.JobTitle,
                WorkProgressPage = "/Applications/WorkProgress/Detail"
            }).ToList()
        };

    public static ReceivedReviewsPanelModel FromEmployer(Dtos.EmployerReceivedReviewsSummaryDto summary) =>
        new()
        {
            AverageRating = summary.AverageRating,
            TotalCount = summary.TotalCount,
            AverageLabel = "Điểm trung bình từ ứng viên",
            EmptyMessage = "Chưa có đánh giá nào từ ứng viên.",
            Items = summary.Items.Select(i => new ReceivedReviewItemPanelModel
            {
                ApplicationId = i.ApplicationId,
                Rating = i.Rating,
                Comment = i.Comment,
                PrimaryLabel = i.ApplicantName,
                SecondaryLabel = i.JobTitle,
                WorkProgressPage = "/Employer/WorkProgress/Detail"
            }).ToList()
        };
}

public class ReceivedReviewItemPanelModel
{
    public long ApplicationId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string PrimaryLabel { get; set; } = "";
    public string SecondaryLabel { get; set; } = "";
    public string WorkProgressPage { get; set; } = "";
}
