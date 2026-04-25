namespace Quizanchos.WebApi.Options;

public class CoinPackageOption
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Coins { get; set; }
    public decimal PriceUSDT { get; set; }
}
