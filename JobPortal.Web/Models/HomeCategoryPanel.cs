namespace JobPortal.Web.Models;

public class HomeCategoryPanel
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public List<string> JobTitles { get; set; } = new();
}
