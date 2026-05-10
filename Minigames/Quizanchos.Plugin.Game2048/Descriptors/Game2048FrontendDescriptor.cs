using Quizanchos.Core;
using Quizanchos.Plugin.Game2048.GameLogic;

namespace Quizanchos.Plugin.Game2048.Descriptors;

public class Game2048FrontendDescriptor : IMinigameFrontendDescriptor
{
    private const string MinigameRoute = "/Minigame/Game2048";
    private const string AssetBase = "/minigames/game2048";

    public int MinigameTypeId => Game2048State.MinigameTypeId;
    public string GameKey => "Game2048";
    public string DisplayName => "2048";
    public bool IsPremium => false;
    public string Description => "Join the tiles and reach 2048.";
    public string CardStyle => "background: linear-gradient(135deg, #edc22e 0%, #f2b179 100%);";
    public string LobbyUrl => MinigameRoute;
    public string GameUrlTemplate => $"{MinigameRoute}/{{gameId}}";
    public string LobbyViewType => "module";
    public string GameViewType => "module";
    public IReadOnlyList<string> LobbyStyles =>
    [
        $"{AssetBase}/css/game2048.css"
    ];
    public IReadOnlyList<string> LobbyScripts =>
    [
        "/js/game-client.js",
        $"{AssetBase}/js/game2048-client.js",
        $"{AssetBase}/js/game2048-lobby.js"
    ];
    public IReadOnlyList<string> GameStyles =>
    [
        $"{AssetBase}/css/game2048.css"
    ];
    public IReadOnlyList<string> GameScripts =>
    [
        "/js/game-client.js",
        $"{AssetBase}/js/game2048-client.js",
        $"{AssetBase}/js/game2048-game.js"
    ];
    public string ActionText => "PLAY →";
    public int Order => 2;
}
