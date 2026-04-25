using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizanchos.WebApi.Constants;
using Quizanchos.WebApi.Dto.TopUp;
using Quizanchos.WebApi.Services.Payment;

namespace Quizanchos.WebApi.Controllers.Payment;

[Route("api/[controller]")]
[ApiController]
[Authorize(AppRole.User)]
public class TopUpController : ControllerBase
{
    private readonly TopUpService _topUpService;

    public TopUpController(TopUpService topUpService)
    {
        _topUpService = topUpService;
    }

    [HttpGet("packages")]
    public IActionResult GetPackages()
    {
        var packages = _topUpService.GetPackages();
        return Ok(packages);
    }

    [HttpPost("create-order")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateTopUpOrderRequestDto request)
    {
        var result = await _topUpService.CreateOrderAsync(User, request.PackageId, request.Network).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("order/{orderId}/status")]
    public async Task<IActionResult> GetOrderStatus(Guid orderId)
    {
        var result = await _topUpService.GetOrderStatusAsync(User, orderId).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingOrders()
    {
        var result = await _topUpService.GetPendingOrdersForUserAsync(User).ConfigureAwait(false);
        return Ok(result);
    }
}
