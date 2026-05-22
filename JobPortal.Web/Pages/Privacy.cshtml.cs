using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages;

public class PrivacyModel : PageModel
{
    public DateTime LastUpdated { get; } = new(2026, 5, 20);

    public void OnGet()
    {
    }
}
