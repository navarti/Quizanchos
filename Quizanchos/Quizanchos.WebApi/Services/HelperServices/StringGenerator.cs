namespace Quizanchos.WebApi.Services.HelperServices;

public static class StringGenerator
{
    public static string GenerateRandomString(Random random, int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, length)
          .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
