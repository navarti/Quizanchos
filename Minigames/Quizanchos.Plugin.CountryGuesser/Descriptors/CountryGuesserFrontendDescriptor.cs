using Quizanchos.Core;
using Quizanchos.Plugin.CountryGuesser.GameLogic;

namespace Quizanchos.Plugin.CountryGuesser.Descriptors;

public sealed class CountryGuesserFrontendDescriptor : IMinigameFrontendDescriptor
{
    private const string Route = "/Minigame/CountryGuesser";
    private const string AssetBase = "/minigames/countryguesser";

    public int MinigameTypeId => CountryGuesserConstants.MinigameTypeId;
    public string GameKey => CountryGuesserConstants.GameKey;
    public string DisplayName => CountryGuesserConstants.DisplayName;
    public bool IsPremium => false;
    public string Description => "Identify highlighted countries on the world map.";
    public string CardStyle => "background: linear-gradient(135deg, #1d976c 0%, #93f9b9 100%);";
    public string LobbyUrl => Route;
    public string GameUrlTemplate => $"{Route}/{{gameId}}";
    public string LobbyViewType => "module";
    public string GameViewType => "module";

    public IReadOnlyList<string> LobbyStyles =>
    [
        $"{AssetBase}/css/country-guesser.css",
    ];
    public IReadOnlyList<string> LobbyScripts =>
    [
        $"{AssetBase}/js/country-guesser-lobby.js",
    ];
    public IReadOnlyList<string> GameStyles =>
    [
        "https://unpkg.com/leaflet@1.9.4/dist/leaflet.css",
        $"{AssetBase}/css/country-guesser.css",
    ];
    public IReadOnlyList<string> GameScripts =>
    [
        "https://unpkg.com/leaflet@1.9.4/dist/leaflet.js",
        $"{AssetBase}/js/country-guesser-game.js",
    ];

    public string ActionText => "PLAY →";
    public int Order => 1200;
}
