using Quizanchos.Common.Enums;
using Quizanchos.Core;

namespace Quizanchos.Quiz.Descriptors;

public class QuizFrontendDescriptor : IMinigameFrontendDescriptor
{
    public string GameKey => nameof(MinigameType.Quiz);
    public string DisplayName => "Quiz";
    public string Description => "Challenge yourself with category-based quizzes.";
    public string CardStyle => "background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);";
    public string LobbyUrl => "/QuizCategories";
    public string GameUrlTemplate => "/Quiz/{gameId}";
    public string ActionText => "START →";
    public int Order => 1;
}
