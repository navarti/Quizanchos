using Microsoft.AspNetCore.Identity;

namespace Quizanchos.WebApi.Util;

public class CriticalExceptionFactory
{
    public static InvalidOperationException CreateIdentityResultException(IdentityResult result)
    {
        return new InvalidOperationException(string.Concat(result.Errors.Select(e => e.Description)));
    }
}
