using System.Collections.Immutable;
using Quizanchos.Core;

namespace Quizanchos.Plugin.Game2048.GameLogic;

public class Game2048Logic : IGameLogic<Game2048State, Game2048Move>
{
    private readonly int _size;
    private readonly Random _random = new();

    public Game2048Logic(int size = 4)
    {
        _size = size;
    }

    public Game2048State CreateInitialState(Guid gameId, ImmutableArray<string> players)
    {
        var state = new Game2048State
        {
            GameId = gameId,
            Players = players.ToList(),
            IsFinished = false,
            Winner = null,
            Size = _size,
            Board = CreateEmptyBoard(_size),
            Score = 0,
            BestTile = 0,
            MoveCount = 0,
            CreationTime = DateTime.UtcNow
        };

        AddRandomTile(state);
        AddRandomTile(state);

        state.BestTile = GetBestTile(state.Board);

        return state;
    }

    public MoveResult ValidateMove(Game2048State state, Game2048Move move, string playerId)
    {
        if (!Enum.IsDefined(move.Direction))
        {
            return MoveResult.Failure("Invalid direction");
        }

        if (!CanMoveInDirection(state.Board, move.Direction))
        {
            return MoveResult.Failure("No tiles can be moved in that direction");
        }

        return MoveResult.Success;
    }

    public void ApplyMove(Game2048State state, Game2048Move move, string playerId)
    {
        int scoreGained = SlideAndMerge(state.Board, move.Direction);
        state.Score += scoreGained;
        state.MoveCount++;

        AddRandomTile(state);

        state.BestTile = GetBestTile(state.Board);
    }

    public bool CheckFinished(Game2048State state)
    {
        return !HasAvailableMoves(state.Board);
    }

    public string? DetermineWinner(Game2048State state)
    {
        // Single-player game; the player always "wins" by their score
        return state.Players.Count > 0 ? state.Players[0] : null;
    }

    public IEnumerable<string> GetExpectedPlayers(Game2048State state)
    {
        return state.Players;
    }

    public bool NeedToFinish(Game2048State state)
    {
        return false;
    }

    public IReadOnlyDictionary<string, int> GetPlayerScores(Game2048State state)
    {
        // Single-player game: award the final board score to the player
        if (state.Players.Count > 0)
        {
            return new Dictionary<string, int> { { state.Players[0], state.Score } };
        }
        return new Dictionary<string, int>();
    }

    // --- Board helpers ---

    private static int[][] CreateEmptyBoard(int size)
    {
        int[][] board = new int[size][];
        for (int i = 0; i < size; i++)
        {
            board[i] = new int[size];
        }
        return board;
    }

    private void AddRandomTile(Game2048State state)
    {
        var emptyCells = new List<(int row, int col)>();
        for (int r = 0; r < state.Size; r++)
        {
            for (int c = 0; c < state.Size; c++)
            {
                if (state.Board[r][c] == 0)
                    emptyCells.Add((r, c));
            }
        }

        if (emptyCells.Count == 0) return;

        var (row, col) = emptyCells[_random.Next(emptyCells.Count)];
        state.Board[row][col] = _random.NextDouble() < 0.9 ? 2 : 4;
    }

    private static int GetBestTile(int[][] board)
    {
        int best = 0;
        foreach (var row in board)
        {
            foreach (var cell in row)
            {
                if (cell > best) best = cell;
            }
        }
        return best;
    }

    private static bool HasAvailableMoves(int[][] board)
    {
        int size = board.Length;
        for (int r = 0; r < size; r++)
        {
            for (int c = 0; c < size; c++)
            {
                if (board[r][c] == 0) return true;
                if (c + 1 < size && board[r][c] == board[r][c + 1]) return true;
                if (r + 1 < size && board[r][c] == board[r + 1][c]) return true;
            }
        }
        return false;
    }

    private static bool CanMoveInDirection(int[][] board, Direction direction)
    {
        int size = board.Length;
        int[][] copy = CopyBoard(board, size);
        int score = SlideAndMerge(copy, direction);
        return !BoardsEqual(board, copy, size);
    }

    private static int[][] CopyBoard(int[][] board, int size)
    {
        int[][] copy = new int[size][];
        for (int i = 0; i < size; i++)
        {
            copy[i] = new int[size];
            Array.Copy(board[i], copy[i], size);
        }
        return copy;
    }

    private static bool BoardsEqual(int[][] a, int[][] b, int size)
    {
        for (int r = 0; r < size; r++)
        {
            for (int c = 0; c < size; c++)
            {
                if (a[r][c] != b[r][c]) return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Slides and merges tiles in the given direction. Returns the score gained.
    /// </summary>
    private static int SlideAndMerge(int[][] board, Direction direction)
    {
        int size = board.Length;
        int totalScore = 0;

        switch (direction)
        {
            case Direction.Left:
                for (int r = 0; r < size; r++)
                    totalScore += SlideRowLeft(board[r]);
                break;
            case Direction.Right:
                for (int r = 0; r < size; r++)
                {
                    Array.Reverse(board[r]);
                    totalScore += SlideRowLeft(board[r]);
                    Array.Reverse(board[r]);
                }
                break;
            case Direction.Up:
                for (int c = 0; c < size; c++)
                {
                    int[] col = ExtractColumn(board, c, size);
                    totalScore += SlideRowLeft(col);
                    SetColumn(board, c, col, size);
                }
                break;
            case Direction.Down:
                for (int c = 0; c < size; c++)
                {
                    int[] col = ExtractColumn(board, c, size);
                    Array.Reverse(col);
                    totalScore += SlideRowLeft(col);
                    Array.Reverse(col);
                    SetColumn(board, c, col, size);
                }
                break;
        }

        return totalScore;
    }

    private static int SlideRowLeft(int[] row)
    {
        int size = row.Length;
        int score = 0;

        // Compact non-zero values to the left
        int[] compact = new int[size];
        int idx = 0;
        for (int i = 0; i < size; i++)
        {
            if (row[i] != 0)
                compact[idx++] = row[i];
        }

        // Merge adjacent equal tiles
        for (int i = 0; i < size - 1; i++)
        {
            if (compact[i] != 0 && compact[i] == compact[i + 1])
            {
                compact[i] *= 2;
                score += compact[i];
                compact[i + 1] = 0;
                i++; // skip the merged tile
            }
        }

        // Compact again after merging
        idx = 0;
        int[] result = new int[size];
        for (int i = 0; i < size; i++)
        {
            if (compact[i] != 0)
                result[idx++] = compact[i];
        }

        Array.Copy(result, row, size);
        return score;
    }

    private static int[] ExtractColumn(int[][] board, int col, int size)
    {
        int[] column = new int[size];
        for (int r = 0; r < size; r++)
            column[r] = board[r][col];
        return column;
    }

    private static void SetColumn(int[][] board, int col, int[] column, int size)
    {
        for (int r = 0; r < size; r++)
            board[r][col] = column[r];
    }
}
