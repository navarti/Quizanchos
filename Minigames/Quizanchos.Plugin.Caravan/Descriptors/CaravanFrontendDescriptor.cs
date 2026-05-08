using Quizanchos.Core;
using Quizanchos.Plugin.Caravan.GameLogic;

namespace Quizanchos.Plugin.Caravan.Descriptors;

public sealed class CaravanFrontendDescriptor : IMinigameFrontendDescriptor
{
    private const string Route = "/Minigame/Caravan";
    private const string AssetBase = "/minigames/caravan";

    public int MinigameTypeId => CaravanConstants.MinigameTypeId;
    public string GameKey => CaravanConstants.GameKey;
    public string DisplayName => CaravanConstants.DisplayName;
    public bool IsPremium => false;
    public string Description => "Build three caravans worth 21-26 each before the AI does (Fallout: New Vegas).";
    public string CardStyle => "background: linear-gradient(135deg, #6b4226 0%, #c9a567 100%);";
    public string LobbyUrl => Route;
    public string GameUrlTemplate => $"{Route}/{{gameId}}";
    public string LobbyViewType => "module";
    public string GameViewType => "module";

    public IReadOnlyList<string> LobbyStyles =>
    [
        $"{AssetBase}/css/caravan.css",
    ];

    public IReadOnlyList<string> LobbyScripts =>
    [
        $"{AssetBase}/js/caravan-lobby.js",
    ];

    public IReadOnlyList<string> GameStyles =>
    [
        $"{AssetBase}/css/caravan.css",
    ];

    public IReadOnlyList<string> GameScripts =>
    [
        $"{AssetBase}/js/caravan-game.js",
    ];

    public string ActionText => "PLAY →";
    public int Order => 1100;
}
