using Quizanchos.Common.Enums;
using Quizanchos.Common.Util;
using Quizanchos.Domain.Entities;

namespace Quizanchos.WebApi.Util;

public static class PremiumQuizCategoryChecker
{
    public static void ThrowIfIsNotAllowed(ApplicationUser user, QuizCategory quizCategory)
    {
        if (quizCategory.IsPremium && user.Status != UserStatusEnum.Premium)
        {
            throw HandledExceptionFactory.Create("You need to be a premium user to access this category");
        }
    }
}
