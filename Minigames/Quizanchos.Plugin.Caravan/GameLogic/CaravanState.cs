using Quizanchos.Core;

namespace Quizanchos.Plugin.Caravan.GameLogic;

public sealed class CaravanState : IGameState
{
    public int MinigameType => CaravanConstants.MinigameTypeId;

    public Guid GameId { get; set; }
    public IReadOnlyList<string> Players { get; set; } = Array.Empty<string>();
    public bool IsFinished { get; set; }
    public string? Winner { get; set; }

    /// <summary>
    /// Two players: index 0 is the human, index 1 is "__caravan_ai__".
    /// </summary>
    public List<CaravanPlayerState> PlayerStates { get; set; } = new();

    /// <summary>
    /// Index into <see cref="PlayerStates"/> of the player whose turn it currently is.
    /// </summary>
    public int CurrentTurnIndex { get; set; }

    /// <summary>
    /// Six caravans total: 0-2 belong to player[0], 3-5 belong to player[1].
    /// </summary>
    public List<CaravanColumn> Columns { get; set; } = new();

    /// <summary>
    /// Discard pile (face-up, public). Used purely for client display; rules don't reference it.
    /// </summary>
    public List<CaravanCard> Discard { get; set; } = new();

    /// <summary>
    /// Number of full opening rounds remaining where players must drop a number card on each
    /// of their three caravans before they can attach faces or play freely.
    /// </summary>
    public int OpeningCardsRemainingP0 { get; set; } = CaravanConstants.CaravansPerPlayer;
    public int OpeningCardsRemainingP1 { get; set; } = CaravanConstants.CaravansPerPlayer;

    public string? LastMoveDescription { get; set; }
}

public sealed class CaravanPlayerState
{
    public string PlayerId { get; set; } = string.Empty;
    public List<CaravanCard> Hand { get; set; } = new();
    public List<CaravanCard> Deck { get; set; } = new();
    public bool IsAi { get; set; }
}
