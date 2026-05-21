namespace JobPortal.Web.Dtos;

public class EmployerPublicProfileDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Image { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public byte? Gender { get; set; }
    public EmployerReceivedReviewsSummaryDto Reviews { get; set; } = new();
}
