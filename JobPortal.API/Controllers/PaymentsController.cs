using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;
using JobPortal.API.Exceptions;
using JobPortal.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IPaymentHistoryService _paymentHistoryService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IConfiguration config,
        IPaymentHistoryService paymentHistoryService,
        ILogger<PaymentsController> logger)
    {
        _config = config;
        _paymentHistoryService = paymentHistoryService;
        _logger = logger;
    }

    [HttpPost("vnpay/create-payment")]
    [Authorize(Roles = "EMPLOYER")]
    public async Task<IActionResult> CreateVnPayPayment(
        [FromBody] CreateVnPayPaymentRequest request,
        CancellationToken cancellationToken)
    {
        if (request.PostingPackageId <= 0)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Gói đăng tin không hợp lệ."));
        }

        var payment = await _paymentHistoryService.CreatePendingPackagePaymentAsync(
            GetCurrentUserId(),
            request.PostingPackageId,
            cancellationToken);

        var tmnCode = GetRequiredConfig("VnPay:TmnCode");
        var hashSecret = GetRequiredConfig("VnPay:HashSecret");
        var baseUrl = GetRequiredConfig("VnPay:BaseUrl");
        var returnUrl = GetRequiredConfig("VnPay:ReturnUrl");

        var vnpay = new VnPayLibrary();
        var txnRef = payment.Id.ToString();

        vnpay.AddRequestData("vnp_Version", "2.1.0");
        vnpay.AddRequestData("vnp_Command", "pay");
        vnpay.AddRequestData("vnp_TmnCode", tmnCode);
        vnpay.AddRequestData("vnp_Amount", ((long)payment.Amount * 100).ToString());
        var createdAt = DateTime.Now;
        vnpay.AddRequestData("vnp_CreateDate", createdAt.ToString("yyyyMMddHHmmss"));
        vnpay.AddRequestData("vnp_CurrCode", "VND");
        vnpay.AddRequestData("vnp_ExpireDate", createdAt.AddMinutes(15).ToString("yyyyMMddHHmmss"));
        vnpay.AddRequestData("vnp_IpAddr", GetIpAddress());
        vnpay.AddRequestData("vnp_Locale", "vn");
        vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toan goi dang tin {payment.Id}");
        vnpay.AddRequestData("vnp_OrderType", "other");
        vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
        vnpay.AddRequestData("vnp_TxnRef", txnRef);

        var paymentUrl = vnpay.CreateRequestUrl(baseUrl, hashSecret);

        _logger.LogInformation("VNPay payment URL created for payment {PaymentHistoryId}.", payment.Id);

        var response = new VnPayPaymentResponse
        {
            Success = true,
            PaymentUrl = paymentUrl,
            PaymentHistoryId = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            TxnRef = txnRef
        };

        return Ok(ApiResponse<VnPayPaymentResponse>.SuccessResponse(response, "VNPay payment URL created successfully."));
    }

    [HttpGet("vnpay-return")]
    [AllowAnonymous]
    public async Task<IActionResult> VnPayReturn(CancellationToken cancellationToken)
    {
        try
        {
            var vnpay = ReadVnPayResponse();
            var hashSecret = GetRequiredConfig("VnPay:HashSecret");
            var secureHash = Request.Query["vnp_SecureHash"].ToString();

            if (!vnpay.ValidateSignature(secureHash, hashSecret))
            {
                return BadRequest(ApiResponse<object>.FailResponse("Chữ ký VNPay không hợp lệ."));
            }

            if (!long.TryParse(vnpay.GetResponseData("vnp_TxnRef"), out var paymentHistoryId))
            {
                return BadRequest(ApiResponse<object>.FailResponse("Mã giao dịch không hợp lệ."));
            }

            var responseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var transactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
            var isSuccessful = responseCode == "00" && transactionStatus == "00";

            var result = await _paymentHistoryService.ConfirmVnPayPaymentAsync(
                paymentHistoryId,
                isSuccessful,
                Request.Query["vnp_TransactionNo"].ToString(),
                Request.Query["vnp_BankCode"].ToString(),
                Request.Query["vnp_BankTranNo"].ToString(),
                responseCode,
                cancellationToken);

            if (!isSuccessful)
            {
                result.Message = GetVnPayResponseMessage(responseCode);
            }

            _logger.LogInformation("VNPay return processed for payment {PaymentHistoryId} with response {ResponseCode}.", paymentHistoryId, responseCode);
            return Ok(ApiResponse<VnPayPaymentResult>.SuccessResponse(result, result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing VNPay return.");
            return BadRequest(ApiResponse<object>.FailResponse(ex.Message));
        }
    }

    [HttpGet("vnpay-ipn")]
    [AllowAnonymous]
    public async Task<IActionResult> VnPayIpn(CancellationToken cancellationToken)
    {
        try
        {
            var vnpay = ReadVnPayResponse();
            var hashSecret = GetRequiredConfig("VnPay:HashSecret");
            var secureHash = Request.Query["vnp_SecureHash"].ToString();

            if (!vnpay.ValidateSignature(secureHash, hashSecret))
            {
                return Ok(CreateIpnResponse("97", "Invalid Signature"));
            }

            if (!long.TryParse(vnpay.GetResponseData("vnp_TxnRef"), out var paymentHistoryId))
            {
                return Ok(CreateIpnResponse("99", "Invalid TxnRef"));
            }

            var responseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var transactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
            var isSuccessful = responseCode == "00" && transactionStatus == "00";

            await _paymentHistoryService.ConfirmVnPayPaymentAsync(
                paymentHistoryId,
                isSuccessful,
                Request.Query["vnp_TransactionNo"].ToString(),
                Request.Query["vnp_BankCode"].ToString(),
                Request.Query["vnp_BankTranNo"].ToString(),
                responseCode,
                cancellationToken);

            _logger.LogInformation("VNPay IPN confirmed payment {PaymentHistoryId} with response {ResponseCode}.", paymentHistoryId, responseCode);
            return Ok(CreateIpnResponse("00", "Confirm Success"));
        }
        catch (NotFoundException)
        {
            return Ok(CreateIpnResponse("01", "Order Not Found"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing VNPay IPN.");
            return Ok(CreateIpnResponse("99", "Unknown error"));
        }
    }

    private static VnPayIpnResponse CreateIpnResponse(string rspCode, string message)
    {
        return new VnPayIpnResponse
        {
            RspCode = rspCode,
            Message = message
        };
    }

    private VnPayLibrary ReadVnPayResponse()
    {
        var vnpay = new VnPayLibrary();
        foreach (var key in Request.Query.Keys)
        {
            if (!string.IsNullOrWhiteSpace(key) && key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase))
            {
                vnpay.AddResponseData(key, Request.Query[key].ToString());
            }
        }

        return vnpay;
    }

    private string GetIpAddress()
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        return string.IsNullOrWhiteSpace(ipAddress) || ipAddress == "::1" ? "127.0.0.1" : ipAddress;
    }

    private string GetRequiredConfig(string key)
    {
        var value = _config[key];
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Missing configuration value: {key}");
        }

        return value.Trim();
    }

    private long GetCurrentUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(sub, out var userId))
        {
            throw new UnauthorizedAccessException("User identifier is missing.");
        }

        return userId;
    }

    private static string GetVnPayResponseMessage(string responseCode)
    {
        return responseCode switch
        {
            "00" => "Giao dịch thành công.",
            "07" => "Giao dịch bị nghi ngờ.",
            "09" => "Thẻ hoặc tài khoản chưa đăng ký Internet Banking.",
            "10" => "Xác thực thông tin thẻ hoặc tài khoản không đúng quá 3 lần.",
            "11" => "Đã hết hạn chờ thanh toán.",
            "12" => "Thẻ hoặc tài khoản bị khóa.",
            "13" => "Sai mật khẩu xác thực giao dịch.",
            "24" => "Khách hàng đã hủy giao dịch.",
            "51" => "Tài khoản không đủ số dư.",
            "65" => "Tài khoản đã vượt quá hạn mức giao dịch trong ngày.",
            "75" => "Ngân hàng thanh toán đang bảo trì.",
            "79" => "Nhập sai mật khẩu thanh toán quá số lần quy định.",
            _ => "Giao dịch thất bại."
        };
    }
}
