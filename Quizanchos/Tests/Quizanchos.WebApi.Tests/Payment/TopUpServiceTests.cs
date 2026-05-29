using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Quizanchos.Common.Util;
using Quizanchos.Domain;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Options;
using Quizanchos.WebApi.Services.Payment;
using Quizanchos.WebApi.Services.Users;
using Xunit;

namespace Quizanchos.WebApi.Tests.Payment;

/// <summary>
/// Unit tests for the Payment service (top-up of the internal balance via USDT) — part of the
/// author's individual work. The tests exercise pure business logic (package catalogue mapping,
/// network/package validation, unique-amount generation) without touching the database, the
/// network or Binance: the only collaborator is an in-memory fake of <c>ITopUpOrderRepository</c>.
/// </summary>
public class TopUpServiceTests
{
    private static readonly List<CoinPackageOption> Packages =
    [
        new() { Id = 1, Name = "Starter", Coins = 100, PriceUSDT = 5.00m },
        new() { Id = 2, Name = "Pro", Coins = 500, PriceUSDT = 20.00m },
    ];

    private static TopUpService CreateService(FakeTopUpOrderRepository repository)
    {
        var binanceOptions = new BinanceApiOptions
        {
            UsdtAddressBep20 = "0xBEP20_WALLET",
            UsdtAddressTrc20 = "TRC20_WALLET",
        };

        return new TopUpService(
            repository,
            userRetrieverService: null!,
            userManager: (UserManager<ApplicationUser>)null!,
            dbContext: (QuizanchosDbContext)null!,
            packages: Microsoft.Extensions.Options.Options.Create(Packages),
            binanceOptions: Microsoft.Extensions.Options.Options.Create(binanceOptions),
            logger: NullLogger<TopUpService>.Instance);
    }

    [Fact]
    public void GetPackages_MapsEveryConfiguredPackageToDto()
    {
        var service = CreateService(new FakeTopUpOrderRepository());

        var result = service.GetPackages();

        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].Id);
        Assert.Equal("Starter", result[0].Name);
        Assert.Equal(100, result[0].Coins);
        Assert.Equal(5.00m, result[0].PriceUSDT);
        Assert.Equal("Pro", result[1].Name);
    }

    [Theory]
    [InlineData("ETH")]
    [InlineData("BTC")]
    [InlineData("")]
    public async Task CreateOrderAsync_UnsupportedNetwork_Throws(string network)
    {
        var service = CreateService(new FakeTopUpOrderRepository());

        await Assert.ThrowsAsync<QuizanchosException>(
            () => service.CreateOrderAsync(new ClaimsPrincipal(), packageId: 1, network));
    }

    [Theory]
    [InlineData("BEP20")]
    [InlineData("trc20")]
    public async Task CreateOrderAsync_UnknownPackage_Throws(string network)
    {
        var service = CreateService(new FakeTopUpOrderRepository());

        await Assert.ThrowsAsync<QuizanchosException>(
            () => service.CreateOrderAsync(new ClaimsPrincipal(), packageId: 999, network));
    }

    [Fact]
    public async Task GenerateUniqueAmount_ReturnsAmountAbovePriceWithFourDecimalNoise()
    {
        var service = CreateService(new FakeTopUpOrderRepository());

        decimal amount = await InvokeGenerateUniqueAmountAsync(service, basePrice: 5.00m, network: "BEP20");

        Assert.True(amount > 5.00m, "Generated amount must be strictly greater than the base price.");
        Assert.True(amount <= 5.0099m, "Noise must stay within the last four decimal digits.");
        // The noise occupies the 4th decimal place, so amount * 10000 is a whole number.
        Assert.Equal(Math.Round(amount * 10000m), amount * 10000m);
    }

    [Fact]
    public async Task GenerateUniqueAmount_ProbesRepositoryForCollisions()
    {
        var repository = new FakeTopUpOrderRepository();
        var service = CreateService(repository);

        decimal amount = await InvokeGenerateUniqueAmountAsync(service, basePrice: 5.00m, network: "BEP20");

        // A free amount is only returned after confirming it is absent among pending orders.
        Assert.Null(await repository.FindPendingByAmountAndNetworkAsync(amount, "BEP20"));
    }

    [Fact]
    public async Task GenerateUniqueAmount_WhenEveryCandidateTaken_Throws()
    {
        var repository = new FakeTopUpOrderRepository();
        for (int k = 1; k <= 99; k++)
        {
            repository.ExistingPendingAmounts.Add(Math.Round(5.00m + k * 0.0001m, 4));
        }
        var service = CreateService(repository);

        await Assert.ThrowsAsync<QuizanchosException>(
            () => InvokeGenerateUniqueAmountAsync(service, basePrice: 5.00m, network: "BEP20"));
    }

    private static async Task<decimal> InvokeGenerateUniqueAmountAsync(TopUpService service, decimal basePrice, string network)
    {
        var method = typeof(TopUpService).GetMethod(
            "GenerateUniqueAmountAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;
        try
        {
            return await (Task<decimal>)method.Invoke(service, [basePrice, network])!;
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            throw ex.InnerException;
        }
    }
}
