using Quizanchos.Common.Util;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Entities.Quiz;

namespace Quizanchos.Quiz.Util;

public static class PremiumQuizCategoryChecker
{
    public static void ThrowIfIsNotAllowed(ApplicationUser user, QuizCategory quizCategory)
    {
        bool hasActivePremium = user.PremiumUntilUtc.HasValue && user.PremiumUntilUtc.Value > DateTime.UtcNow;

        if (quizCategory.IsPremium && !hasActivePremium)
        {
            throw HandledExceptionFactory.Create("You need to be a premium user to access this category");
        }
    }
}
