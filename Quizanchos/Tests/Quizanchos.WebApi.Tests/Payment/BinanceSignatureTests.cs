using System.Reflection;
using Quizanchos.WebApi.Services.Payment;
using Xunit;

namespace Quizanchos.WebApi.Tests.Payment;

/// <summary>
/// Unit tests for the HMAC-SHA256 request signing used by <see cref="BinanceDepositService"/>
/// to authenticate calls to the Binance Sapi deposit-history endpoint. The signing routine is a
/// pure, deterministic function, so it is verified against an independently computed reference
/// vector without any network access.
/// </summary>
public class BinanceSignatureTests
{
    // HMAC-SHA256(key: "mysecret", data: "coin=USDT&network=BEP20&timestamp=1700000000000")
    // computed independently (lowercase hex).
    private const string ReferenceData = "coin=USDT&network=BEP20&timestamp=1700000000000";
    private const string ReferenceSecret = "mysecret";
    private const string ReferenceSignature = "a99439bf34d177e81f8557e029d2d9dfa13afdb83eb7f8259f3c9135a5b3539e";

    private static string ComputeSignature(string data, string secret)
    {
        var method = typeof(BinanceDepositService).GetMethod(
            "ComputeHmacSha256", BindingFlags.NonPublic | BindingFlags.Static)!;
        return (string)method.Invoke(null, [data, secret])!;
    }

    [Fact]
    public void ComputeHmacSha256_MatchesReferenceVector()
    {
        string signature = ComputeSignature(ReferenceData, ReferenceSecret);

        Assert.Equal(ReferenceSignature, signature);
    }

    [Fact]
    public void ComputeHmacSha256_IsDeterministic()
    {
        string first = ComputeSignature(ReferenceData, ReferenceSecret);
        string second = ComputeSignature(ReferenceData, ReferenceSecret);

        Assert.Equal(first, second);
    }

    [Fact]
    public void ComputeHmacSha256_DifferentSecretProducesDifferentSignature()
    {
        string signature = ComputeSignature(ReferenceData, ReferenceSecret);
        string other = ComputeSignature(ReferenceData, "different-secret");

        Assert.NotEqual(signature, other);
    }

    [Fact]
    public void ComputeHmacSha256_ReturnsLowercaseHex64Chars()
    {
        string signature = ComputeSignature(ReferenceData, ReferenceSecret);

        Assert.Equal(64, signature.Length);
        Assert.All(signature, c => Assert.True(
            (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f'),
            $"Unexpected character '{c}' in signature; expected lowercase hex."));
    }
}
