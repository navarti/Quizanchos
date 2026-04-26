using Quizanchos.Core;

namespace Quizanchos.TicketToRideEurope.GameLogic;

public class TicketToRideEuropeState : IGameState
{
    public const int MinigameTypeId = 4;

    public int MinigameType => MinigameTypeId;
    public Guid GameId { get; set; }
    public IReadOnlyList<string> Players { get; set; } = Array.Empty<string>();
    public bool IsFinished { get; set; }
    public string? Winner { get; set; }

    public string Phase { get; set; } = PhaseInit;
    public const string PhaseInit = "init";
    public const string PhasePlay = "play";
    public const string PhaseEnded = "ended";

    public string? PendingAction { get; set; }
    public const string PendingDrawSecondCard = "drawSecondCard";
    public const string PendingTunnelDecision = "tunnelDecision";
    public const string PendingKeepDrawnTickets = "keepDrawnTickets";

    public int CurrentPlayerIndex { get; set; }
    public bool LastRoundTriggered { get; set; }
    public string? LastRoundTriggeredBy { get; set; }
    public int FinalTurnsRemaining { get; set; }
    public DateTime CreationTime { get; set; }
    public int TurnNumber { get; set; }

    public List<string> TrainDeck { get; set; } = new();
    public List<string> TrainDiscard { get; set; } = new();
    public List<string?> FaceUp { get; set; } = new() { null, null, null, null, null };

    public List<TicketCard> TicketDeck { get; set; } = new();
    public List<TicketCard> TicketDiscard { get; set; } = new();

    public List<PlayerInfo> PlayerStates { get; set; } = new();

    public List<ClaimedRouteInfo> ClaimedRoutes { get; set; } = new();
    public List<StationInfo> Stations { get; set; } = new();

    public PendingTunnelData? PendingTunnel { get; set; }

    public List<LogEntry> RecentLog { get; set; } = new();

    public int NextStationCost { get; set; } = 1;

    public class PlayerInfo
    {
        public string PlayerId { get; set; } = "";
        public string Color { get; set; } = "";
        public int Order { get; set; }
        public int TrainsRemaining { get; set; } = 45;
        public int StationsBuilt { get; set; }
        public int Score { get; set; }
        public Dictionary<string, int> Hand { get; set; } = new();
        public List<TicketCard> Tickets { get; set; } = new();
        public List<TicketCard> PendingTickets { get; set; } = new();
        public int PendingMinKeep { get; set; }
        public bool HasPickedInitialTickets { get; set; }
        public bool HasTakenFinalTurn { get; set; }
        public bool HasResigned { get; set; }
    }

    public class TicketCard
    {
        public string Id { get; set; } = "";
        public string CityA { get; set; } = "";
        public string CityB { get; set; } = "";
        public int Points { get; set; }
        public bool IsLong { get; set; }
    }

    public class ClaimedRouteInfo
    {
        public string RouteId { get; set; } = "";
        public string PlayerId { get; set; } = "";
        public string ColorUsed { get; set; } = "";
        public int LocomotivesUsed { get; set; }
    }

    public class StationInfo
    {
        public string CityId { get; set; } = "";
        public string PlayerId { get; set; } = "";
    }

    public class PendingTunnelData
    {
        public string PlayerId { get; set; } = "";
        public string RouteId { get; set; } = "";
        public string ColorUsed { get; set; } = "";
        public int LocomotivesPaid { get; set; }
        public int ColorCardsPaid { get; set; }
        public List<string> RevealedCards { get; set; } = new();
        public int ExtraColorCardsRequired { get; set; }
        public int ExtraLocomotivesRequired { get; set; }
    }

    public class LogEntry
    {
        public DateTime Time { get; set; }
        public string PlayerId { get; set; } = "";
        public string Message { get; set; } = "";
    }
}
