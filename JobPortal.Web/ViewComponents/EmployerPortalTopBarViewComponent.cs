using JobPortal.Web.Dtos;
using JobPortal.Web.Models;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace JobPortal.Web.ViewComponents;

public class EmployerPortalTopBarViewComponent : ViewComponent
{
    private readonly ApiService _api;
    private readonly EmployerSupportOptions _support;

    public EmployerPortalTopBarViewComponent(
        ApiService api,
        IOptions<EmployerSupportOptions> support)
    {
        _api = api;
        _support = support.Value;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var nav = await _api.GetApiDataAsync<EmployerPortalNavDto>("/api/employers/me/portal-nav")
            ?? new EmployerPortalNavDto();

        var userName = HttpContext.Session.GetString("UserName") ?? "Tài khoản";
        var avatarUrl = HttpContext.Session.GetString("UserAvatarUrl");

        return View(new EmployerPortalTopBarViewModel
        {
            Nav = nav,
            Support = _support,
            UserName = userName,
            UserAvatarUrl = avatarUrl,
            UserInitials = AccountPanelViewModel.GetInitials(userName)
        });
    }
}

public class EmployerPortalTopBarViewModel
{
    public EmployerPortalNavDto Nav { get; init; } = new();
    public EmployerSupportOptions Support { get; init; } = new();
    public string UserName { get; init; } = "";
    public string? UserAvatarUrl { get; init; }
    public string UserInitials { get; init; } = "?";
}
