using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quizanchos.Common.Util;
using Quizanchos.Domain;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.WebApi.Dto.TopUp;
using Quizanchos.WebApi.Options;
using Quizanchos.WebApi.Services.Users;

namespace Quizanchos.WebApi.Services.Payment;

public class TopUpService
{
    private const int OrderExpiryMinutes = 30;
    private static readonly string[] ValidNetworks = ["BEP20", "TRC20"];

    private readonly ITopUpOrderRepository _orderRepository;
    private readonly UserRetrieverService _userRetrieverService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly QuizanchosDbContext _dbContext;
    private readonly List<CoinPackageOption> _packages;
    private readonly BinanceApiOptions _binanceOptions;
    private readonly ILogger<TopUpService> _logger;

    public TopUpService(
        ITopUpOrderRepository orderRepository,
        UserRetrieverService userRetrieverService,
        UserManager<ApplicationUser> userManager,
        QuizanchosDbContext dbContext,
        IOptions<List<CoinPackageOption>> packages,
        IOptions<BinanceApiOptions> binanceOptions,
        ILogger<TopUpService> logger)
    {
        _orderRepository = orderRepository;
        _userRetrieverService = userRetrieverService;
        _userManager = userManager;
        _dbContext = dbContext;
        _packages = packages.Value;
        _binanceOptions = binanceOptions.Value;
        _logger = logger;
    }

    public List<CoinPackageDto> GetPackages()
    {
        return _packages.Select(p => new CoinPackageDto(p.Id, p.Name, p.Coins, p.PriceUSDT)).ToList();
    }

    public async Task<CreateTopUpOrderResultDto> CreateOrderAsync(ClaimsPrincipal claimsPrincipal, int packageId, string network)
    {
        if (!ValidNetworks.Contains(network, StringComparer.OrdinalIgnoreCase))
        {
            throw HandledExceptionFactory.Create("Invalid network. Supported: BEP20, TRC20.");
        }

        var package = _packages.FirstOrDefault(p => p.Id == packageId)
            ?? throw HandledExceptionFactory.Create("Invalid package.");

        string userId = _userRetrieverService.GetUserId(claimsPrincipal);
        string networkUpper = network.ToUpperInvariant();

        decimal uniqueAmount = await GenerateUniqueAmountAsync(package.PriceUSDT, networkUpper).ConfigureAwait(false);

        string walletAddress = networkUpper switch
        {
            "BEP20" => _binanceOptions.UsdtAddressBep20,
            "TRC20" => _binanceOptions.UsdtAddressTrc20,
            _ => throw HandledExceptionFactory.Create("Invalid network.")
        };

        var order = new TopUpOrder
        {
            Id = Guid.NewGuid(),
            ApplicationUserId = userId,
            Status = TopUpOrderStatus.Pending,
            CoinsToCredit = package.Coins,
            AmountUSDT = uniqueAmount,
            Network = networkUpper,
            CreatedAtUtc = DateTime.UtcNow,
        };

        await _orderRepository.Create(order).ConfigureAwait(false);

        return new CreateTopUpOrderResultDto(
            order.Id,
            walletAddress,
            uniqueAmount,
            networkUpper,
            order.CreatedAtUtc.AddMinutes(OrderExpiryMinutes));
    }

    public async Task<TopUpOrderStatusDto> GetOrderStatusAsync(ClaimsPrincipal claimsPrincipal, Guid orderId)
    {
        string userId = _userRetrieverService.GetUserId(claimsPrincipal);
        var order = await _orderRepository.GetById(orderId).ConfigureAwait(false);

        if (order.ApplicationUserId != userId)
        {
            throw HandledExceptionFactory.Create("Order not found.");
        }

        var user = await _userRetrieverService.GetUserByClaims(claimsPrincipal).ConfigureAwait(false);

        return new TopUpOrderStatusDto(order.Id, order.Status.ToString(), user.Coins);
    }

    public async Task<List<PendingTopUpOrderDto>> GetPendingOrdersForUserAsync(ClaimsPrincipal claimsPrincipal)
    {
        string userId = _userRetrieverService.GetUserId(claimsPrincipal);
        var orders = await _orderRepository.GetPendingByUserIdAsync(userId).ConfigureAwait(false);

        return orders.Select(o => new PendingTopUpOrderDto(
            o.Id, o.AmountUSDT, o.Network, o.CoinsToCredit,
            o.CreatedAtUtc, o.CreatedAtUtc.AddMinutes(OrderExpiryMinutes))).ToList();
    }

    public async Task<List<AdminPendingOrderDto>> GetAllPendingOrdersAsync()
    {
        var orders = await _orderRepository.GetAllPendingAsync().ConfigureAwait(false);

        return orders.Select(o => new AdminPendingOrderDto(
            o.Id, o.ApplicationUserId, o.ApplicationUser.UserName ?? "",
            o.AmountUSDT, o.Network, o.CoinsToCredit, o.CreatedAtUtc)).ToList();
    }

    public async Task<List<AdminOrderHistoryDto>> GetOrderHistoryAsync(int take = 50)
    {
        var orders = await _orderRepository.GetNonPendingAsync(take).ConfigureAwait(false);

        return orders.Select(o => new AdminOrderHistoryDto(
            o.Id, o.ApplicationUserId, o.ApplicationUser.UserName ?? "",
            o.AmountUSDT, o.Network, o.CoinsToCredit, o.Status.ToString(),
            o.CreatedAtUtc, o.CompletedAtUtc)).ToList();
    }

    public async Task ConfirmOrderAsync(Guid orderId, string? txId)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);

        try
        {
            var order = await _orderRepository.GetById(orderId).ConfigureAwait(false);

            if (order.Status != TopUpOrderStatus.Pending)
            {
                throw HandledExceptionFactory.Create("Order is not in pending state.");
            }

            var user = await _dbContext.ApplicationUsers
                .FirstOrDefaultAsync(x => x.Id == order.ApplicationUserId)
                .ConfigureAwait(false)
                ?? throw HandledExceptionFactory.Create("User not found.");

            user.Coins += order.CoinsToCredit;
            order.Status = TopUpOrderStatus.ManuallyConfirmed;
            order.CompletedAtUtc = DateTime.UtcNow;
            order.BinanceTxId = txId;

            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);

            _logger.LogInformation("Manually confirmed top-up order {OrderId} for user {UserId}, credited {Coins} coins",
                orderId, order.ApplicationUserId, order.CoinsToCredit);
        }
        catch
        {
            await transaction.RollbackAsync().ConfigureAwait(false);
            throw;
        }
    }

    public async Task ProcessDetectedDepositAsync(decimal amount, string network, string txId)
    {
        if (await _orderRepository.ExistsByBinanceTxIdAsync(txId).ConfigureAwait(false))
        {
            return;
        }

        var order = await _orderRepository.FindPendingByAmountAndNetworkAsync(amount, network).ConfigureAwait(false);
        if (order is null)
        {
            return;
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync().ConfigureAwait(false);

        try
        {
            var user = await _dbContext.ApplicationUsers
                .FirstOrDefaultAsync(x => x.Id == order.ApplicationUserId)
                .ConfigureAwait(false);

            if (user is null)
            {
                _logger.LogWarning("User {UserId} not found for top-up order {OrderId}", order.ApplicationUserId, order.Id);
                return;
            }

            user.Coins += order.CoinsToCredit;
            order.Status = TopUpOrderStatus.Completed;
            order.CompletedAtUtc = DateTime.UtcNow;
            order.BinanceTxId = txId;

            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
            await transaction.CommitAsync().ConfigureAwait(false);

            _logger.LogInformation("Auto-detected deposit {TxId}, credited {Coins} coins to user {UserId}",
                txId, order.CoinsToCredit, order.ApplicationUserId);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync().ConfigureAwait(false);
            _logger.LogError(ex, "Failed to process deposit {TxId} for order {OrderId}", txId, order.Id);
            throw;
        }
    }

    public async Task ExpireStaleOrdersAsync()
    {
        DateTime cutoff = DateTime.UtcNow.AddMinutes(-OrderExpiryMinutes);
        var staleOrders = await _orderRepository.GetExpiredCandidatesAsync(cutoff).ConfigureAwait(false);

        foreach (var order in staleOrders)
        {
            order.Status = TopUpOrderStatus.Expired;
        }

        if (staleOrders.Count > 0)
        {
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
            _logger.LogInformation("Expired {Count} stale top-up orders", staleOrders.Count);
        }
    }

    private async Task<decimal> GenerateUniqueAmountAsync(decimal basePrice, string network)
    {
        var random = new Random();

        for (int attempt = 0; attempt < 50; attempt++)
        {
            decimal suffix = random.Next(1, 100) * 0.0001m;
            decimal candidate = Math.Round(basePrice + suffix, 4);

            var existing = await _orderRepository.FindPendingByAmountAndNetworkAsync(candidate, network).ConfigureAwait(false);
            if (existing is null)
            {
                return candidate;
            }
        }

        throw HandledExceptionFactory.Create("Unable to generate unique payment amount. Please try again later.");
    }
}
