using Microsoft.AspNetCore.Identity;

namespace Quizanchos.WebApi.Util;

public class CriticalExceptionFactory
{
    public static Exception Create(string errorMessage)
    {
        throw new Exception(errorMessage);
    }

    public static ArgumentException CreateConfigException(string configPropertyName)
    {
        throw new ArgumentException($"Configuration property '{configPropertyName}' is missing");
    }

    public static InvalidOperationException CreateIdentityResultException(IdentityResult result)
    {
        return new InvalidOperationException(string.Concat(result.Errors.Select(e => e.Description)));
    }
}
