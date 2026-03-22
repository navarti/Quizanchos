using Quizanchos.Common.Enums;
using Quizanchos.Core;

namespace Quizanchos.QuizMultiplayer.Descriptors;

public class QuizMultiplayerFrontendDescriptor : IMinigameFrontendDescriptor
{
    private const string MinigameRoute = "/Minigame/QuizMultiplayer";

    public int MinigameTypeId => 3;
    public string GameKey => "QuizMultiplayer";
    public string DisplayName => "Quiz Multiplayer";
    public string Description => "Team up and compete in real-time quizzes.";
    public string CardStyle => "background: linear-gradient(135deg, #7b61ff 0%, #4fc3f7 100%);";
    public string LobbyUrl => MinigameRoute;
    public string GameUrlTemplate => $"{MinigameRoute}/{{gameId}}";
    public string LobbyViewType => "module";
    public string GameViewType => "module";
    public IReadOnlyList<string> LobbyStyles =>
    [
        "/minigames/quiz-multiplayer/css/quiz-multiplayer-lobby.css"
    ];
    public IReadOnlyList<string> LobbyScripts =>
    [
        "/js/room-lobby-base.js",
        "/minigames/quiz-multiplayer/js/quiz-multiplayer-lobby.js"
    ];
    public IReadOnlyList<string> GameStyles =>
    [
        "/minigames/quiz-multiplayer/css/quiz-multiplayer.css"
    ];
    public IReadOnlyList<string> GameScripts =>
    [
        "/js/game-client.js",
        "/minigames/quiz-multiplayer/js/quiz-multiplayer-client.js",
        "/minigames/quiz-multiplayer/js/quiz-multiplayer-game.js"
    ];
    public string ActionText => "FIND ROOM →";
    public int Order => 3;
}
