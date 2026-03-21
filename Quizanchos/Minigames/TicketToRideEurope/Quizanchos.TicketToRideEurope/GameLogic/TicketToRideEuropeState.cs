using Quizanchos.Core;

namespace Quizanchos.TicketToRideEurope.GameLogic;

public class TicketToRideEuropeState : IGameState
{
    public int MinigameType => 4;

    public Guid GameId { get; set; }
    public IReadOnlyList<string> Players { get; set; } = Array.Empty<string>();
    public bool IsFinished { get; set; }
    public string? Winner { get; set; }

    public string CurrentPlayerId { get; set; } = string.Empty;
    public int CurrentPlayerIndex { get; set; }
    public int TurnNumber { get; set; }

    public bool LastRoundTriggered { get; set; }
    public int RemainingFinalTurns { get; set; }

    public List<TrainCardColor> Deck { get; set; } = new();
    public List<TrainCardColor> FaceUpCards { get; set; } = new();
    public List<DestinationTicket> DestinationDeck { get; set; } = new();

    public List<RouteState> Routes { get; set; } = new();
    public Dictionary<string, PlayerState> PlayerStates { get; set; } = new();

    public List<DestinationTicket> PendingDestinationChoices { get; set; } = new();
    public bool AwaitingTicketSelection { get; set; }

    public string LastActionSummary { get; set; } = string.Empty;

    public DateTime CreationTime { get; set; }

    public class PlayerState
    {
        public string PlayerId { get; set; } = string.Empty;
        public int TrainsRemaining { get; set; } = 45;
        public int StationsRemaining { get; set; } = 3;
        public int Score { get; set; }

        public List<TrainCardColor> Hand { get; set; } = new();
        public List<DestinationTicket> DestinationTickets { get; set; } = new();
        public List<string> ClaimedRouteIds { get; set; } = new();
        public List<string> StationCities { get; set; } = new();
    }

    public class RouteState
    {
        public string Id { get; set; } = string.Empty;
        public string CityA { get; set; } = string.Empty;
        public string CityB { get; set; } = string.Empty;
        public int Length { get; set; }
        public TrainCardColor? Color { get; set; }
        public bool IsTunnel { get; set; }
        public int FerryLocomotivesRequired { get; set; }
        public string? ClaimedBy { get; set; }
    }

    public class DestinationTicket
    {
        public string CityA { get; set; } = string.Empty;
        public string CityB { get; set; } = string.Empty;
        public int Points { get; set; }
    }
}

public enum TrainCardColor
{
    Red = 0,
    Blue = 1,
    Green = 2,
    Yellow = 3,
    Black = 4,
    White = 5,
    Orange = 6,
    Purple = 7,
    Locomotive = 8
}
