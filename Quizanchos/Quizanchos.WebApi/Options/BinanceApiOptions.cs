namespace Quizanchos.WebApi.Options;

public class BinanceApiOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public string UsdtAddressBep20 { get; set; } = string.Empty;
    public string UsdtAddressTrc20 { get; set; } = string.Empty;
}
