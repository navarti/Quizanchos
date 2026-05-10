using Quizanchos.Core;
using Quizanchos.Plugin.CaravanMultiplayer.GameLogic;

namespace Quizanchos.Plugin.CaravanMultiplayer.Descriptors;

public sealed class CaravanMpFrontendDescriptor : IMinigameFrontendDescriptor
{
    private const string Route = "/Minigame/CaravanMultiplayer";
    private const string AssetBase = "/minigames/caravanmultiplayer";

    public int MinigameTypeId => CaravanMpConstants.MinigameTypeId;
    public string GameKey => CaravanMpConstants.GameKey;
    public string DisplayName => CaravanMpConstants.DisplayName;
    public bool IsPremium => false;
    public bool IsMultiplayer => true;
    public string Description => "Build three caravans worth 21-26 each before your opponent does (Fallout: New Vegas, head-to-head).";
    public string CardStyle => "background: linear-gradient(135deg, #6b4226 0%, #b85c9e 100%);";
    public string LobbyUrl => Route;
    public string GameUrlTemplate => $"{Route}/{{gameId}}";
    public string LobbyViewType => "module";
    public string GameViewType => "module";

    public IReadOnlyList<string> LobbyStyles =>
    [
        $"{AssetBase}/css/caravan-mp.css",
    ];

    public IReadOnlyList<string> LobbyScripts =>
    [
        "/js/room-lobby-base.js",
        $"{AssetBase}/js/caravan-mp-lobby.js",
    ];

    public IReadOnlyList<string> GameStyles =>
    [
        $"{AssetBase}/css/caravan-mp.css",
    ];

    public IReadOnlyList<string> GameScripts =>
    [
        $"{AssetBase}/js/caravan-mp-game.js",
    ];

    public string ActionText => "FIND ROOM →";
    public int Order => 1101;
}
