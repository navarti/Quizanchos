namespace Quizanchos.WebApi.Util;

public static class ConfigurationHelper
{
    public static string GetOption(this IConfiguration configuration, string optionName)
    {
        string? optionValue = configuration[optionName];
        if (string.IsNullOrEmpty(optionValue))
        {
            throw CriticalExceptionFactory.CreateConfigException(optionName);
        }

        return optionValue;
    }
}
