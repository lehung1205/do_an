namespace JobPortal.API.DTOs;

public class PostingPackageDto
{
    public long Id { get; set; }
    public long AdminId { get; set; }
    public string Name { get; set; } = null!;
    public int Price { get; set; }
    public int PostingLimit { get; set; }
}
