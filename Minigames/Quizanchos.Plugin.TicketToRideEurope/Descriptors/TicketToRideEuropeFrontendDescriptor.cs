using Quizanchos.Core;

namespace Quizanchos.Plugin.TicketToRideEurope.Descriptors;

public class TicketToRideEuropeFrontendDescriptor : IMinigameFrontendDescriptor
{
    private const string MinigameRoute = "/Minigame/TicketToRideEurope";

    public int MinigameTypeId => GameLogic.TicketToRideEuropeState.MinigameTypeId;
    public string GameKey => "TicketToRideEurope";
    public string DisplayName => "Ticket to Ride: Europe";
    public bool IsPremium => true;
    public bool IsMultiplayer => true;
    public string Description => "Connect European cities by claiming train routes. Collect cards, complete destinations, and outscore your rivals.";
    public string CardStyle => "background: linear-gradient(135deg, #2c5d3f 0%, #c97f3f 100%);";
    public string LobbyUrl => MinigameRoute;
    public string GameUrlTemplate => $"{MinigameRoute}/{{gameId}}";
    public string LobbyViewType => "module";
    public string GameViewType => "module";

    private const string AssetBase = "/minigames/tickettorideeurope";

    public IReadOnlyList<string> LobbyStyles =>
    [
        $"{AssetBase}/css/ticket-to-ride-europe-lobby.css"
    ];
    public IReadOnlyList<string> LobbyScripts =>
    [
        "/js/room-lobby-base.js",
        $"{AssetBase}/js/ticket-to-ride-europe-lobby.js"
    ];
    public IReadOnlyList<string> GameStyles =>
    [
        $"{AssetBase}/css/ticket-to-ride-europe.css"
    ];
    public IReadOnlyList<string> GameScripts =>
    [
        "/js/game-client.js",
        $"{AssetBase}/js/ticket-to-ride-europe-client.js",
        $"{AssetBase}/js/ticket-to-ride-europe-game.js"
    ];
    public string ActionText => "FIND ROOM →";
    public int Order => 4;
}
