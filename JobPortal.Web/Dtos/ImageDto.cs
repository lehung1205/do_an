namespace JobPortal.Web.Dtos;

public class ImageDto
{
    public long Id { get; set; }
    public long JobId { get; set; }
    public string Url { get; set; } = null!;
    public string? Name { get; set; }
}
