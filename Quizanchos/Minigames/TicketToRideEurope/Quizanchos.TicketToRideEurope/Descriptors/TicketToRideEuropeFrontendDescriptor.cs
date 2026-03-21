using Quizanchos.Core;

namespace Quizanchos.TicketToRideEurope.Descriptors;

public class TicketToRideEuropeFrontendDescriptor : IMinigameFrontendDescriptor
{
    private const string MinigameRoute = "/Minigame/TicketToRideEurope";

    public int MinigameTypeId => 4;
    public string GameKey => "TicketToRideEurope";
    public string DisplayName => "Ticket to Ride: Europe";
    public string Description => "Claim routes, complete destination tickets, and build the longest path across Europe.";
    public string CardStyle => "background: linear-gradient(135deg, #1f5d9d 0%, #1fa67a 100%);";
    public string LobbyUrl => MinigameRoute;
    public string GameUrlTemplate => $"{MinigameRoute}/{{gameId}}";
    public string LobbyViewType => "module";
    public string GameViewType => "module";

    public IReadOnlyList<string> LobbyStyles =>
    [
        "/minigames/ticket-to-ride-europe/css/ttr-europe.css"
    ];

    public IReadOnlyList<string> LobbyScripts =>
    [
        "https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/8.0.7/signalr.min.js",
        "/js/room-lobby-base.js",
        "/minigames/ticket-to-ride-europe/js/ttr-europe-lobby.js"
    ];

    public IReadOnlyList<string> GameStyles =>
    [
        "/minigames/ticket-to-ride-europe/css/ttr-europe.css"
    ];

    public IReadOnlyList<string> GameScripts =>
    [
        "/js/game-client.js",
        "/minigames/ticket-to-ride-europe/js/ttr-europe-client.js",
        "/minigames/ticket-to-ride-europe/js/ttr-europe-game.js"
    ];

    public string ActionText => "BUILD ROUTES →";
    public int Order => 4;
}
