namespace JobPortal.Web.Models;

public class PageBackLinkModel
{
    public required string PagePath { get; init; }

    public required string Label { get; init; }

    public bool EmployerVariant { get; init; }

    public string NavClass { get; init; } = "page-back-nav";
}
