namespace JobPortal.Web.Dtos;

public class ResumeDto
{
    public long Id { get; set; }
    public long JobSeekerId { get; set; }
    public string Title { get; set; } = null!;
    public string Url { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class CreateResumeRequest
{
    public string Title { get; set; } = null!;
    public string Url { get; set; } = null!;
}
