namespace JobPortal.Web.Dtos;

public class EmployerJobEditDto
{
    public long Id { get; set; }
    public long CategoryId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Salary { get; set; } = null!;
    public string Location { get; set; } = null!;
    public string? WorkingHours { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string PostingStatus { get; set; } = null!;
    public bool CanEdit { get; set; }
    public IReadOnlyList<ImageDto> Images { get; set; } = Array.Empty<ImageDto>();
}

public class UpdateEmployerJobRequest
{
    public long CategoryId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Salary { get; set; } = null!;
    public string Location { get; set; } = null!;
    public string? WorkingHours { get; set; }
    public DateTime ExpiryDate { get; set; }
}
