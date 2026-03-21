using Quizanchos.Common.Enums;
using Quizanchos.Core;

namespace Quizanchos.Game2048.Descriptors;

public class Game2048FrontendDescriptor : IMinigameFrontendDescriptor
{
    private const string MinigameRoute = "/Minigame/Game2048";

    public int MinigameTypeId => 2;
    public string GameKey => "Game2048";
    public string DisplayName => "2048";
    public string Description => "Join the tiles and reach 2048.";
    public string CardStyle => "background: linear-gradient(135deg, #edc22e 0%, #f2b179 100%);";
    public string LobbyUrl => MinigameRoute;
    public string GameUrlTemplate => $"{MinigameRoute}/{{gameId}}";
    public string LobbyViewType => "module";
    public string GameViewType => "module";
    public IReadOnlyList<string> LobbyStyles =>
    [
        "/minigames/game2048/css/game2048.css"
    ];
    public IReadOnlyList<string> LobbyScripts =>
    [
        "/js/game-client.js",
        "/minigames/game2048/js/game2048-client.js",
        "/minigames/game2048/js/game2048-lobby.js"
    ];
    public IReadOnlyList<string> GameStyles =>
    [
        "/minigames/game2048/css/game2048.css"
    ];
    public IReadOnlyList<string> GameScripts =>
    [
        "/js/game-client.js",
        "/minigames/game2048/js/game2048-client.js",
        "/minigames/game2048/js/game2048-game.js"
    ];
    public string ActionText => "PLAY →";
    public int Order => 2;
}
