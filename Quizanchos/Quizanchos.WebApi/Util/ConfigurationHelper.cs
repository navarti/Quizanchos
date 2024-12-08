using Microsoft.IdentityModel.Tokens;

namespace Quizanchos.WebApi.Util;

public static class ConfigurationHelper
{
    public static string GetOption(this IConfiguration configuration, string optionName)
    {
        string? optionValue = configuration[optionName];
        if (optionValue.IsNullOrEmpty())
        {
            throw CriticalExceptionFactory.CreateConfigException(optionName);
        }

        return optionValue;
    }
}
