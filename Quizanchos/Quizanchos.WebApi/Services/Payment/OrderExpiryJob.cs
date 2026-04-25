using Quartz;

namespace Quizanchos.WebApi.Services.Payment;

public class OrderExpiryJob : IJob
{
    private readonly TopUpService _topUpService;
    private readonly ILogger<OrderExpiryJob> _logger;

    public OrderExpiryJob(TopUpService topUpService, ILogger<OrderExpiryJob> logger)
    {
        _topUpService = topUpService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            await _topUpService.ExpireStaleOrdersAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to expire stale top-up orders");
        }
    }
}
