using JobPortal.Web.Dtos;
using JobPortal.Web.Dtos.Common;
using JobPortal.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobPortal.Web.Pages.Employer;

public class PaymentReturnModel : PageModel
{
    private readonly ApiService _api;

    public PaymentReturnModel(ApiService api)
    {
        _api = api;
    }

    public VnPayPaymentResult? Result { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            var response = await _api.GetApiResponseAsync<VnPayPaymentResult>(
                $"/api/payments/vnpay-return{Request.QueryString}");

            Result = response?.Data;
            IsSuccess = response is { Success: true, Data.Success: true };
            Message = Result?.Message ?? response?.Message ?? "Khong the xac nhan ket qua thanh toan.";
        }
        catch (HttpRequestException)
        {
            IsSuccess = false;
            Message = "Khong ket noi duoc API de xac nhan thanh toan.";
        }
    }
}

