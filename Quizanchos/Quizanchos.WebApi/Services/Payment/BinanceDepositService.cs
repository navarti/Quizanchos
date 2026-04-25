using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Quizanchos.WebApi.Options;

namespace Quizanchos.WebApi.Services.Payment;

public class BinanceDepositService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly BinanceApiOptions _options;
    private readonly ILogger<BinanceDepositService> _logger;

    public BinanceDepositService(
        IHttpClientFactory httpClientFactory,
        IOptions<BinanceApiOptions> options,
        ILogger<BinanceDepositService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<List<BinanceDeposit>> GetRecentDepositsAsync(string coin, string network, long startTimeMs)
    {
        var client = _httpClientFactory.CreateClient("Binance");

        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        string queryString = $"coin={coin}&network={network}&startTime={startTimeMs}&status=1&timestamp={timestamp}";
        string signature = ComputeHmacSha256(queryString, _options.ApiSecret);
        string url = $"/sapi/v1/capital/deposit/hisrec?{queryString}&signature={signature}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("X-MBX-APIKEY", _options.ApiKey);

        using var response = await client.SendAsync(request).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            _logger.LogWarning("Binance deposit history API returned {StatusCode}: {Body}", response.StatusCode, body);
            return [];
        }

        string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var deposits = JsonSerializer.Deserialize<List<BinanceDeposit>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        });

        return deposits ?? [];
    }

    private static string ComputeHmacSha256(string data, string secret)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(secret);
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);
        using var hmac = new HMACSHA256(keyBytes);
        byte[] hash = hmac.ComputeHash(dataBytes);
        return Convert.ToHexStringLower(hash);
    }
}

public class BinanceDeposit
{
    public decimal Amount { get; set; }
    public string Coin { get; set; } = string.Empty;
    public string Network { get; set; } = string.Empty;
    public int Status { get; set; }
    public string TxId { get; set; } = string.Empty;
    public long InsertTime { get; set; }
}
