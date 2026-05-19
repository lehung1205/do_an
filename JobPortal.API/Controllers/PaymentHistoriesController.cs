using JobPortal.API.DTOs;
using JobPortal.API.DTOs.Common;
using JobPortal.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentHistoriesController : ControllerBase
{
    private readonly IPaymentHistoryService _paymentHistoryService;

    public PaymentHistoriesController(IPaymentHistoryService paymentHistoryService)
    {
        _paymentHistoryService = paymentHistoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaymentHistories(CancellationToken cancellationToken)
    {
        var items = await _paymentHistoryService.GetAllPaymentHistoriesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PaymentHistoryDto>>.SuccessResponse(items, "Payment histories retrieved successfully."));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetPaymentHistory(long id, CancellationToken cancellationToken)
    {
        var item = await _paymentHistoryService.GetPaymentHistoryByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<PaymentHistoryDto>.SuccessResponse(item, "Payment history retrieved successfully."));
    }

    [HttpPost]
    public async Task<IActionResult> CreatePaymentHistory([FromBody] PaymentHistoryDto dto, CancellationToken cancellationToken)
    {
        var created = await _paymentHistoryService.CreatePaymentHistoryAsync(dto, cancellationToken);
        return CreatedAtAction(
            nameof(GetPaymentHistory),
            new { id = created.Id },
            ApiResponse<PaymentHistoryDto>.SuccessResponse(created, "Payment history created successfully."));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdatePaymentHistory(long id, [FromBody] PaymentHistoryDto dto, CancellationToken cancellationToken)
    {
        await _paymentHistoryService.UpdatePaymentHistoryAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Payment history updated successfully."));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeletePaymentHistory(long id, CancellationToken cancellationToken)
    {
        await _paymentHistoryService.DeletePaymentHistoryAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Payment history deleted successfully."));
    }
}
