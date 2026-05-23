using Quizanchos.Core;

namespace Quizanchos.Plugin.CaravanMultiplayer.GameLogic;

public sealed class CaravanMpState : IGameState
{
    public int MinigameType => CaravanMpConstants.MinigameTypeId;

    public Guid GameId { get; set; }
    public IReadOnlyList<string> Players { get; set; } = Array.Empty<string>();
    public bool IsFinished { get; set; }
    public string? Winner { get; set; }

    public List<CaravanMpPlayerState> PlayerStates { get; set; } = new();

    public int CurrentTurnIndex { get; set; }

    public List<CaravanMpColumn> Columns { get; set; } = new();

    public List<CaravanMpCard> Discard { get; set; } = new();

    public string? LastMoveDescription { get; set; }

    public string? SurrenderedPlayerId { get; set; }

    public Dictionary<string, string> PlayerNicknames { get; set; } = new();
}

public sealed class CaravanMpPlayerState
{
    public string PlayerId { get; set; } = string.Empty;
    public List<CaravanMpCard> Hand { get; set; } = new();
    public List<CaravanMpCard> Deck { get; set; } = new();
}
