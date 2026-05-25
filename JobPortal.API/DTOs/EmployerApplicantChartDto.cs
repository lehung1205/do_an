namespace JobPortal.API.DTOs;

public class EmployerApplicantChartDto
{
    public int Days { get; set; }
    public IReadOnlyList<EmployerApplicantChartPointDto> Points { get; set; } = Array.Empty<EmployerApplicantChartPointDto>();
}

public class EmployerApplicantChartPointDto
{
    public string Label { get; set; } = null!;
    public int Count { get; set; }
}
