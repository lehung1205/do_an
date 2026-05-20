using JobPortal.Web.Dtos;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages.Employer;

public class BuyPackagesModel : PageModel
{
    private readonly ApiService _api;

    public BuyPackagesModel(ApiService api) => _api = api;

    public List<PostingPackageDto> Packages { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var redirect = RequireEmployerLogin();
        if (redirect != null)
        {
            return redirect;
        }

        Packages = await _api.GetApiDataAsync<List<PostingPackageDto>>("/api/postingpackages") ?? new();
        if (Packages.Count == 0)
        {
            ErrorMessage = "Hiện chưa có gói đăng tin. Vui lòng quay lại sau.";
        }

        return Page();
    }

    private IActionResult? RequireEmployerLogin()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
        {
            return RedirectToPage("/Auth/Login", new { returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString });
        }

        if (!string.Equals(HttpContext.Session.GetString("UserRole"), "EMPLOYER", StringComparison.Ordinal))
        {
            return RedirectToPage("/Index");
        }

        return null;
    }
}
