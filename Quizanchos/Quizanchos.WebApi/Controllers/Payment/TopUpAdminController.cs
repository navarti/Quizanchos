using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizanchos.WebApi.Constants;
using Quizanchos.WebApi.Services.Payment;

namespace Quizanchos.WebApi.Controllers.Payment;

[Route("api/admin/topup")]
[ApiController]
[Authorize(AppRole.Admin)]
public class TopUpAdminController : ControllerBase
{
    private readonly TopUpService _topUpService;

    public TopUpAdminController(TopUpService topUpService)
    {
        _topUpService = topUpService;
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingOrders()
    {
        var result = await _topUpService.GetAllPendingOrdersAsync().ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("{orderId}/confirm")]
    public async Task<IActionResult> ConfirmOrder(Guid orderId, [FromQuery] string? txId)
    {
        await _topUpService.ConfirmOrderAsync(orderId, txId).ConfigureAwait(false);
        return Ok(new { Message = "Order confirmed and coins credited." });
    }
}
