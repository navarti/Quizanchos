using Quizanchos.Core;

namespace Quizanchos.Plugin.Game2048.GameLogic;

public class Game2048State : IGameState
{
    public const int MinigameTypeId = 1300;

    public int MinigameType => MinigameTypeId;

    public Guid GameId { get; set; }
    public IReadOnlyList<string> Players { get; set; } = Array.Empty<string>();
    public bool IsFinished { get; set; }
    public string? Winner { get; set; }

    public int Size { get; set; } = 4;
    public int[][] Board { get; set; } = Array.Empty<int[]>();
    public int Score { get; set; }
    public int BestTile { get; set; }
    public int MoveCount { get; set; }
    public DateTime CreationTime { get; set; }

    public Game2048State()
    {
    }
}
