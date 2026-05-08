using Quizanchos.Core;
using Quizanchos.Plugin.ClickCounter.GameLogic;

namespace Quizanchos.Plugin.ClickCounter.Descriptors;

public sealed class ClickCounterFrontendDescriptor : IMinigameFrontendDescriptor
{
    private const string Route = "/Minigame/ClickCounter";
    private const string AssetBase = "/minigames/clickcounter";

    public int MinigameTypeId => ClickCounterConstants.MinigameTypeId;
    public string GameKey => ClickCounterConstants.GameKey;
    public string DisplayName => "Click Counter";
    public bool IsPremium => false;
    public string Description => "Click as fast as you can to reach the target. (Sample 3rd-party plugin.)";
    public string CardStyle => "background: linear-gradient(135deg, #6366f1 0%, #8b5cf6 100%);";
    public string LobbyUrl => Route;
    public string GameUrlTemplate => $"{Route}/{{gameId}}";
    public string LobbyViewType => "module";
    public string GameViewType => "module";

    public IReadOnlyList<string> LobbyStyles =>
    [
        $"{AssetBase}/css/click-counter.css"
    ];

    public IReadOnlyList<string> LobbyScripts =>
    [
        $"{AssetBase}/js/click-counter-lobby.js"
    ];

    public IReadOnlyList<string> GameStyles =>
    [
        $"{AssetBase}/css/click-counter.css"
    ];

    public IReadOnlyList<string> GameScripts =>
    [
        $"{AssetBase}/js/click-counter-game.js"
    ];

    public string ActionText => "PLAY →";
    public int Order => 1000;
}
