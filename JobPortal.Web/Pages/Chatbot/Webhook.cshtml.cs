using System.Net.Http.Json;
using JobPortal.Web.Models;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace JobPortal.Web.Pages.Chatbot;

[IgnoreAntiforgeryToken]
public class WebhookModel : PageModel
{
    internal const string UnsupportedTopicFallbackReply =
        "Xin lỗi, chúng tôi chưa hỗ trợ thông tin này. Bạn vui lòng thử hỏi về tìm việc làm, đăng ký tài khoản hoặc các nội dung khác trên JobPortal.";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly N8nChatbotOptions _options;
    private readonly ILogger<WebhookModel> _logger;

    public WebhookModel(
        IHttpClientFactory httpClientFactory,
        IOptions<N8nChatbotOptions> options,
        ILogger<WebhookModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public IActionResult OnGet() => NotFound();

    public async Task<IActionResult> OnPostAsync([FromBody] ChatbotMessageRequest? request)
    {
        var isLoggedIn = !string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"));
        var sessionRole = HttpContext.Session.GetString("UserRole");

        if (!IsChatbotAllowed(isLoggedIn, sessionRole))
        {
            return new JsonResult(new { success = false, error = "Trợ lý ảo không khả dụng cho tài khoản này." })
            { StatusCode = 403 };
        }

        if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.WebhookUrl))
        {
            return new JsonResult(new { success = false, error = "Chatbot chưa được cấu hình." }) { StatusCode = 503 };
        }

        if (request == null || string.IsNullOrWhiteSpace(request.Message))
        {
            return new JsonResult(new { success = false, error = "Tin nhắn không được để trống." }) { StatusCode = 400 };
        }

        var urlError = ValidateWebhookUrl(_options.WebhookUrl);
        if (urlError != null)
        {
            return new JsonResult(new { success = false, error = urlError }) { StatusCode = 400 };
        }

        var message = request.Message.Trim();
        var n8nRole = MapRoleForN8n(isLoggedIn ? sessionRole : null);
        var isAdmin = string.Equals(sessionRole?.Trim(), "ADMIN", StringComparison.OrdinalIgnoreCase);
        var accessToken = isAdmin ? HttpContext.Session.GetString("JwtToken") : null;

        var payload = isAdmin
            ? N8nChatbotWebhookPayload.ForAdmin(message, n8nRole, accessToken)
            : N8nChatbotWebhookPayload.ForUser(message, n8nRole);

        _logger.LogInformation(
            "n8n webhook payload: role={Role}, sessionRole={SessionRole}, message={Message}, hasAccessToken={HasAccessToken}, accessTokenLength={TokenLength}",
            payload.Role,
            sessionRole,
            payload.Message,
            !string.IsNullOrEmpty(payload.AccessToken),
            payload.AccessToken?.Length ?? 0);

        try
        {
            var client = _httpClientFactory.CreateClient("N8nWebhook");
            using var response = await client.PostAsJsonAsync(_options.WebhookUrl, payload);

            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "n8n webhook returned {StatusCode}: {Body}",
                    (int)response.StatusCode,
                    body);

                var error = (int)response.StatusCode == 404
                    ? BuildN8n404HelpMessage(_options.WebhookUrl)
                    : $"Webhook trả về lỗi {(int)response.StatusCode}. Kiểm tra workflow n8n đang chạy.";

                return new JsonResult(new { success = false, error })
                { StatusCode = 502 };
            }

            var reply = N8nChatbotResponseParser.ExtractReply(body);
            if (string.IsNullOrWhiteSpace(reply))
            {
                _logger.LogWarning(
                    "n8n webhook returned 200 but reply could not be parsed. BodyLength={BodyLength}, BodyPreview={BodyPreview}",
                    body.Length,
                    body.Length > 500 ? body[..500] + "…" : body);

                reply = UnsupportedTopicFallbackReply;
            }

            return new JsonResult(new { success = true, reply });
        }
        catch (TaskCanceledException)
        {
            return new JsonResult(new
            {
                success = false,
                error = "Hết thời gian chờ phản hồi. Workflow n8n có thể đang xử lý lâu — thử lại sau."
            })
            { StatusCode = 504 };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to reach n8n webhook at {Url}", _options.WebhookUrl);
            return new JsonResult(new
            {
                success = false,
                error = "Không kết nối được tới n8n (localhost:5678). Hãy bật n8n và kích hoạt workflow."
            })
            { StatusCode = 502 };
        }
    }

    /// <summary>Chatbot chỉ cho khách (chưa đăng nhập), ADMIN và JOB_SEEKER.</summary>
    public static bool IsChatbotAllowed(bool isLoggedIn, string? sessionRole)
    {
        if (!isLoggedIn)
        {
            return true;
        }

        return sessionRole?.Trim().ToUpperInvariant() is "ADMIN" or "JOB_SEEKER";
    }

    public static string MapRoleForN8n(string? sessionRole) =>
        sessionRole?.Trim().ToUpperInvariant() switch
        {
            "JOB_SEEKER" => "employee",
            "ADMIN" => "admin",
            _ => "guest"
        };

    internal static string? ValidateWebhookUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return "Chưa cấu hình N8nChatbot:WebhookUrl trong appsettings.json.";
        }

        if (url.Contains("/workflow/", StringComparison.OrdinalIgnoreCase))
        {
            return "WebhookUrl sai: /workflow/... là link mở workflow trên trình duyệt, không phải Webhook URL. "
                + "Mở node Webhook trong n8n → copy URL dạng http://localhost:5678/webhook/... hoặc /webhook-test/...";
        }

        if (!url.Contains("/webhook", StringComparison.OrdinalIgnoreCase))
        {
            return "WebhookUrl nên chứa /webhook hoặc /webhook-test (URL từ node Webhook trong n8n).";
        }

        return null;
    }

    private static string BuildN8n404HelpMessage(string webhookUrl) =>
        "n8n trả 404 — URL webhook không tồn tại hoặc workflow chưa lắng nghe. "
        + "Kiểm tra: (1) Copy đúng URL từ node Webhook (không phải /workflow/... trên thanh địa chỉ). "
        + "(2) Production: bật workflow Active. "
        + "(3) Test: bấm «Listen for test event» hoặc «Execute workflow» rồi dùng URL /webhook-test/.... "
        + $"URL hiện tại: {webhookUrl}";

    public sealed class ChatbotMessageRequest
    {
        public string Message { get; set; } = null!;
    }
}
