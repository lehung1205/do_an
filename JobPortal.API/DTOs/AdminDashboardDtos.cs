namespace JobPortal.API.DTOs;

public class AdminDashboardDto
{
    public AdminDashboardSummaryDto Summary { get; set; } = new();
    public IReadOnlyList<AdminRatedUserDto> TopEmployers { get; set; } = Array.Empty<AdminRatedUserDto>();
    public IReadOnlyList<AdminRatedUserDto> TopJobSeekers { get; set; } = Array.Empty<AdminRatedUserDto>();
    public AdminRecruitmentChartsDto Charts { get; set; } = new();
}

public class AdminDashboardSummaryDto
{
    public int PendingJobsCount { get; set; }
    public int ApprovedJobsCount { get; set; }
    public int RejectedJobsCount { get; set; }
    public int ClosedJobsCount { get; set; }
    public int ActiveUsersCount { get; set; }
    public int TotalEmployers { get; set; }
    public int TotalJobSeekers { get; set; }
    public int TotalApplications { get; set; }
}

public class AdminRatedUserDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Email { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
}

public class AdminRecruitmentChartsDto
{
    public IReadOnlyList<AdminChartPointDto> RecruitmentTrend { get; set; } = Array.Empty<AdminChartPointDto>();
    public IReadOnlyList<AdminChartPointDto> MonthlyApplications { get; set; } = Array.Empty<AdminChartPointDto>();
    public IReadOnlyList<AdminChartPointDto> JobsByStatus { get; set; } = Array.Empty<AdminChartPointDto>();
    public IReadOnlyList<AdminCategorySliceDto> JobsByCategory { get; set; } = Array.Empty<AdminCategorySliceDto>();
}

public class AdminChartPointDto
{
    public string Label { get; set; } = null!;
    public int Value { get; set; }
}

public class AdminCategorySliceDto
{
    public string CategoryName { get; set; } = null!;
    public int Count { get; set; }
}
