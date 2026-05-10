using Quizanchos.Core;
using Quizanchos.Plugin.CountryGuesserMultiplayer.GameLogic;

namespace Quizanchos.Plugin.CountryGuesserMultiplayer.Descriptors;

public sealed class CountryGuesserMpFrontendDescriptor : IMinigameFrontendDescriptor
{
    private const string Route = "/Minigame/CountryGuesserMultiplayer";
    private const string AssetBase = "/minigames/countryguessermultiplayer";

    public int MinigameTypeId => CountryGuesserMpConstants.MinigameTypeId;
    public string GameKey => CountryGuesserMpConstants.GameKey;
    public string DisplayName => CountryGuesserMpConstants.DisplayName;
    public bool IsPremium => false;
    public bool IsMultiplayer => true;
    public string Description => "Race other players to identify highlighted countries on the world map.";
    public string CardStyle => "background: linear-gradient(135deg, #134e5e 0%, #71b280 100%);";
    public string LobbyUrl => Route;
    public string GameUrlTemplate => $"{Route}/{{gameId}}";
    public string LobbyViewType => "module";
    public string GameViewType => "module";

    public IReadOnlyList<string> LobbyStyles =>
    [
        $"{AssetBase}/css/country-guesser-mp.css",
    ];
    public IReadOnlyList<string> LobbyScripts =>
    [
        "/js/room-lobby-base.js",
        $"{AssetBase}/js/country-guesser-mp-lobby.js",
    ];
    public IReadOnlyList<string> GameStyles =>
    [
        "https://unpkg.com/leaflet@1.9.4/dist/leaflet.css",
        $"{AssetBase}/css/country-guesser-mp.css",
    ];
    public IReadOnlyList<string> GameScripts =>
    [
        "https://unpkg.com/leaflet@1.9.4/dist/leaflet.js",
        "/js/game-client.js",
        $"{AssetBase}/js/country-guesser-mp-game.js",
    ];

    public string ActionText => "FIND ROOM →";
    public int Order => 1201;
}
