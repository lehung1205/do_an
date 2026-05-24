using JobPortal.Web.Dtos;
using JobPortal.Web.Dtos.Auth;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages.Messages;

public class IndexModel : PageModel
{
    private readonly ApiService _api;
    private readonly IConfiguration _config;

    public IndexModel(ApiService api, IConfiguration config)
    {
        _api = api;
        _config = config;
    }

    public List<ChatThreadDto> Threads { get; set; } = new();
    public int TotalUnreadCount { get; set; }
    public string? ErrorMessage { get; set; }
    public string HubUrl { get; set; } = "";
    public string ApiBaseUrl { get; set; } = "";
    public string AccessToken { get; set; } = "";
    public long? SelectedPartnerUserId { get; set; }
    public long CurrentUserId { get; set; }

    public async Task<IActionResult> OnGetAsync(long? applicationId, long? partnerUserId)
    {
        var redirect = RequireChatUser();
        if (redirect != null)
        {
            return redirect;
        }
        AccessToken = HttpContext.Session.GetString("JwtToken") ?? "";

        ApiBaseUrl = _config["ApiSettings:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:5068";
        var hubPath = _config["ApiSettings:ChatHubPath"] ?? "/hubs/chat";
        HubUrl = $"{ApiBaseUrl}{hubPath}";

        if (!long.TryParse(HttpContext.Session.GetString("UserId"), out var userId))
        {
            var profile = await _api.GetApiDataAsync<ProfileResponse>("/api/auth/me");
            if (profile != null)
            {
                userId = profile.Id;
                HttpContext.Session.SetString("UserId", profile.Id.ToString());
            }
        }

        CurrentUserId = userId;

        var threads = await _api.GetApiDataAsync<List<ChatThreadDto>>("/api/chat/threads");
        if (threads == null)
        {
            ErrorMessage = "Không tải được danh sách hội thoại.";
            return Page();
        }

        Threads = threads;
        TotalUnreadCount = threads.Sum(t => t.UnreadCount);

        if (partnerUserId > 0)
        {
            SelectedPartnerUserId = partnerUserId;
        }
        else if (applicationId > 0)
        {
            var match = threads.FirstOrDefault(t =>
                t.ApplicationId == applicationId
                || t.ApplicationIds.Contains(applicationId.Value));
            SelectedPartnerUserId = match?.PartnerUserId;
        }

        return Page();
    }

    private IActionResult? RequireChatUser()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
        {
            return RedirectToPage("/Auth/Login", new
            {
                returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString
            });
        }

        var role = HttpContext.Session.GetString("UserRole");
        if (!string.Equals(role, "EMPLOYER", StringComparison.Ordinal) &&
            !string.Equals(role, "JOB_SEEKER", StringComparison.Ordinal))
        {
            return RedirectToPage("/Index");
        }

        return null;
    }
}
