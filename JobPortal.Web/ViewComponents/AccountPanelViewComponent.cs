using JobPortal.Web.Dtos.Auth;
using JobPortal.Web.Models;
using JobPortal.Web.Pages.Account;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.Web.ViewComponents;

public class AccountPanelViewComponent : ViewComponent
{
    private readonly ApiService _api;
    private readonly IAntiforgery _antiforgery;

    public AccountPanelViewComponent(ApiService api, IAntiforgery antiforgery)
    {
        _api = api;
        _antiforgery = antiforgery;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
        {
            return Content(string.Empty);
        }

        var profile = await _api.GetApiDataAsync<ProfileResponse>("/api/auth/me");
        if (profile == null)
        {
            return Content(string.Empty);
        }

        var request = HttpContext.Request;
        var returnUrl = $"{request.PathBase}{request.Path}{request.QueryString}";
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);

        ViewData.TemplateInfo.HtmlFieldPrefix = string.Empty;

        var model = new AccountPanelViewModel
        {
            Profile = profile,
            ReturnUrl = returnUrl,
            SuccessMessage = TempData["AccountSuccessMessage"] as string,
            ErrorMessage = TempData["AccountErrorMessage"] as string,
            ActiveTab = TempData["AccountTab"] as string ?? "view",
            UpdateFormAction = Url.Page("/Account/Index", pageHandler: "Update") ?? "/Account/Index?handler=Update",
            ChangePasswordFormAction = Url.Page("/Account/Index", pageHandler: "ChangePassword") ?? "/Account/Index?handler=ChangePassword",
            AntiForgeryFieldName = tokens.FormFieldName,
            AntiForgeryRequestToken = tokens.RequestToken ?? string.Empty,
            EditInput = new IndexModel.EditInputModel
            {
                Name = profile.Name,
                PhoneNumber = profile.PhoneNumber
            }
        };

        return View(model);
    }
}
