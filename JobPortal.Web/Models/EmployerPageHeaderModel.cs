namespace JobPortal.Web.Models;

public class EmployerPageHeaderModel
{
    public string Title { get; init; } = "";
    public string? Subtitle { get; init; }
    public string? BreadcrumbLabel { get; init; }
    public string? CtaText { get; init; }
    public string? CtaPage { get; init; }
    public string CtaIconClass { get; init; } = "bi-plus-lg";

    public string DisplayBreadcrumb => string.IsNullOrWhiteSpace(BreadcrumbLabel) ? Title : BreadcrumbLabel!;
}
