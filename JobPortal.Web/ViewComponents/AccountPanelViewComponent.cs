using JobPortal.Web.Dtos;
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

        IReadOnlyList<ResumeDto> resumes = Array.Empty<ResumeDto>();
        if (string.Equals(profile.Role, "JOB_SEEKER", StringComparison.Ordinal))
        {
            resumes = await _api.GetApiDataAsync<List<ResumeDto>>("/api/resumes/me") ?? new List<ResumeDto>();
        }

        var resumeInput = new IndexModel.ResumeInputModel();
        var tempData = ViewContext?.TempData;
        if (tempData != null)
        {
            if (tempData.TryGetValue("ResumeInputTitle", out var resumeTitle) && resumeTitle is string t)
            {
                resumeInput.Title = t;
            }
        }

        var model = new AccountPanelViewModel
        {
            Profile = profile,
            ReturnUrl = returnUrl,
            SuccessMessage = TempData["AccountSuccessMessage"] as string,
            ErrorMessage = TempData["AccountErrorMessage"] as string,
            ActiveTab = TempData["AccountTab"] as string ?? "view",
            Resumes = resumes,
            UpdateFormAction = Url.Page("/Account/Index", pageHandler: "Update") ?? "/Account/Index?handler=Update",
            ChangePasswordFormAction = Url.Page("/Account/Index", pageHandler: "ChangePassword") ?? "/Account/Index?handler=ChangePassword",
            AddResumeFormAction = Url.Page("/Account/Index", pageHandler: "AddResume") ?? "/Account/Index?handler=AddResume",
            DeleteResumeFormAction = Url.Page("/Account/Index", pageHandler: "DeleteResume") ?? "/Account/Index?handler=DeleteResume",
            AntiForgeryFieldName = tokens.FormFieldName,
            AntiForgeryRequestToken = tokens.RequestToken ?? string.Empty,
            EditInput = IndexModel.CreateEditInputFromProfile(profile),
            ResumeInput = resumeInput
        };

        return View(model);
    }
}
