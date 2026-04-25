using Microsoft.Extensions.Options;
using Quartz;
using Quizanchos.WebApi.Options;

namespace Quizanchos.WebApi.Services.Payment;

public class DepositCheckerJob : IJob
{
    private static readonly string[] Networks = ["BEP20", "TRC20"];

    private readonly BinanceDepositService _binanceService;
    private readonly TopUpService _topUpService;
    private readonly BinanceApiOptions _options;
    private readonly ILogger<DepositCheckerJob> _logger;

    public DepositCheckerJob(
        BinanceDepositService binanceService,
        TopUpService topUpService,
        IOptions<BinanceApiOptions> options,
        ILogger<DepositCheckerJob> logger)
    {
        _binanceService = binanceService;
        _topUpService = topUpService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        if (string.IsNullOrEmpty(_options.ApiKey) || string.IsNullOrEmpty(_options.ApiSecret))
        {
            return;
        }

        long startTimeMs = DateTimeOffset.UtcNow.AddMinutes(-35).ToUnixTimeMilliseconds();

        foreach (string network in Networks)
        {
            try
            {
                var deposits = await _binanceService.GetRecentDepositsAsync("USDT", network, startTimeMs).ConfigureAwait(false);

                foreach (var deposit in deposits)
                {
                    try
                    {
                        await _topUpService.ProcessDetectedDepositAsync(deposit.Amount, network, deposit.TxId).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to process deposit {TxId}", deposit.TxId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch {Network} deposits from Binance", network);
            }
        }
    }
}
