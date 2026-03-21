using Quizanchos.Core;

namespace Quizanchos.Quiz.Descriptors;

public class QuizFrontendDescriptor : IMinigameFrontendDescriptor
{
    private const string MinigameRoute = "/Minigame/Quiz";

    public int MinigameTypeId => 1;
    public string GameKey => "Quiz";
    public string DisplayName => "Quiz";
    public string Description => "Challenge yourself with category-based quizzes.";
    public string CardStyle => "background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);";
    public string LobbyUrl => MinigameRoute;
    public string GameUrlTemplate => $"{MinigameRoute}/{{gameId}}";
    public string LobbyViewType => "module";
    public string GameViewType => "module";
    public IReadOnlyList<string> LobbyStyles =>
    [
        "/minigames/quiz/css/quiz.css",
        "/minigames/quiz/css/quizcategories.css",
        "/css/bootstrap.min.css"
    ];
    public IReadOnlyList<string> LobbyScripts =>
    [
        "/js/game-client.js",
        "/minigames/quiz/js/quiz-client.js",
        "/js/common.js",
        "/minigames/quiz/js/quiz-lobby.js"
    ];
    public IReadOnlyList<string> GameStyles =>
    [
        "/minigames/quiz/css/quiz-options.css"
    ];
    public IReadOnlyList<string> GameScripts =>
    [
        "/js/game-client.js",
        "/minigames/quiz/js/quiz-client.js",
        "/js/common.js",
        "/minigames/quiz/js/quiz-game.js"
    ];
    public string ActionText => "START →";
    public int Order => 1;
}
