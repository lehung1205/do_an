using JobPortal.Web.Dtos;
using JobPortal.Web.Models;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.Web.ViewComponents;

public class EmployerPortalNavViewComponent : ViewComponent
{
    private readonly ApiService _api;

    public EmployerPortalNavViewComponent(ApiService api) => _api = api;

    public async Task<IViewComponentResult> InvokeAsync(string activeKey = "")
    {
        var nav = await _api.GetApiDataAsync<EmployerPortalNavDto>("/api/employers/me/portal-nav")
            ?? new EmployerPortalNavDto();

        return View(new EmployerPortalNavViewModel
        {
            ActiveKey = activeKey ?? "",
            Nav = nav
        });
    }
}

public class EmployerPortalNavViewModel
{
    public string ActiveKey { get; init; } = "";
    public EmployerPortalNavDto Nav { get; init; } = new();

    public bool IsActive(string key) =>
        string.Equals(ActiveKey, key, StringComparison.OrdinalIgnoreCase);
}
