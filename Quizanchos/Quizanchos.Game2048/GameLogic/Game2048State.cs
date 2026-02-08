using Quizanchos.Common.Enums;
using Quizanchos.Core;

namespace Quizanchos.Game2048.GameLogic;

public class Game2048State : IGameState
{
    public MinigameType MinigameType => MinigameType.Game2048;

    public Guid GameId { get; set; }
    public IReadOnlyList<string> Players { get; set; } = Array.Empty<string>();
    public bool IsFinished { get; set; }
    public string? Winner { get; set; }

    // 2048-specific state
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
