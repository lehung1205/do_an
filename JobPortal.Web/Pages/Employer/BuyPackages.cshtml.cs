using System.Globalization;
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

    public int CurrentPostingLimit { get; set; }

    public string? CompanyName { get; set; }

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        return await LoadPageAsync();
    }

    public async Task<IActionResult> OnPostBuyAsync(long packageId)
    {
        var redirect = RequireEmployerLogin();
        if (redirect != null)
        {
            return redirect;
        }

        if (packageId <= 0)
        {
            ErrorMessage = "Gói đăng tin không hợp lệ. Vui lòng chọn lại.";
            return await LoadPageAsync();
        }

        try
        {
            var response = await _api.PostApiResponseAsync<CreateVnPayPaymentRequest, VnPayPaymentResponse>(
                "/api/payments/vnpay/create-payment",
                new CreateVnPayPaymentRequest { PostingPackageId = packageId });

            if (response is { Success: true, Data.PaymentUrl.Length: > 0 })
            {
                return Redirect(response.Data.PaymentUrl);
            }

            var detail = response?.Errors.FirstOrDefault(e => !string.IsNullOrWhiteSpace(e.Message))?.Message;
            ErrorMessage = detail ?? response?.Message ?? "Không thể tạo giao dịch VNPay. Vui lòng thử lại.";
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "Không kết nối được API tại http://localhost:5068. Vui lòng chạy lại JobPortal.API rồi thử lại.";
        }

        return await LoadPageAsync();
    }

    private async Task<IActionResult> LoadPageAsync()
    {
        var redirect = RequireEmployerLogin();
        if (redirect != null)
        {
            return redirect;
        }

        var dashboardTask = _api.GetApiDataAsync<EmployerDashboardDto>("/api/employers/me/dashboard");
        var packagesTask = _api.GetApiDataAsync<List<PostingPackageDto>>("/api/postingpackages");
        await Task.WhenAll(dashboardTask, packagesTask);

        var dashboard = await dashboardTask;
        CurrentPostingLimit = dashboard?.PostingLimit ?? 0;
        CompanyName = dashboard?.CompanyName;

        Packages = await packagesTask ?? new();
        Packages = Packages.OrderBy(p => p.Price).ToList();

        if (Packages.Count == 0)
        {
            ErrorMessage = "Hiện chưa có gói đăng tin. Vui lòng quay lại sau.";
        }

        return Page();
    }

    public static string FormatMoney(int amount) =>
        amount.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " ₫";

    public static string FormatPricePerPost(int price, int postingLimit)
    {
        if (postingLimit <= 0)
        {
            return "—";
        }

        var perPost = (double)price / postingLimit;
        return perPost.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " ₫/tin";
    }

    public long? GetBestValuePackageId()
    {
        if (Packages.Count == 0)
        {
            return null;
        }

        return Packages
            .OrderBy(p => (double)p.Price / Math.Max(1, p.PostingLimit))
            .First()
            .Id;
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
