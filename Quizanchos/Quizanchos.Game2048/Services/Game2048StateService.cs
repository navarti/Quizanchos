using System.Text.Json;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Entities.Game2048;
using Quizanchos.Domain.Repositories.Game2048.Interfaces;
using Quizanchos.Game2048.GameLogic;

namespace Quizanchos.Game2048.Services;

public class Game2048StateService
{
    private readonly IGame2048SessionRepository _repository;

    public Game2048StateService(IGame2048SessionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Game2048State?> LoadStateAsync(Guid gameSessionId)
    {
        Game2048SessionState? sessionState = await _repository.GetByGameSessionIdAsync(gameSessionId);
        if (sessionState == null)
            return null;

        return ConvertToGameState(sessionState);
    }

    public async Task SaveStateAsync(Guid gameSessionId, Game2048State state)
    {
        Game2048SessionState? existingState = await _repository.GetByGameSessionIdAsync(gameSessionId);
        if (existingState == null)
        {
            throw new InvalidOperationException($"Game2048SessionState not found for GameSessionId: {gameSessionId}");
        }

        existingState.BoardJson = JsonSerializer.Serialize(state.Board);
        existingState.Score = state.Score;
        existingState.BestTile = state.BestTile;
        existingState.MoveCount = state.MoveCount;

        existingState.GameSession.IsFinished = state.IsFinished;
        if (!string.IsNullOrEmpty(state.Winner))
        {
            existingState.GameSession.WinnerId = state.Winner;
            existingState.GameSession.FinishedAt = DateTime.UtcNow;
        }
        if (state.IsFinished)
        {
            existingState.GameSession.IsActive = false;
        }

        await _repository.UpdateAsync(existingState);
    }

    public async Task<Game2048SessionState> CreateInitialStateAsync(
        GameSession gameSession,
        int size,
        int[][] initialBoard)
    {
        var sessionState = new Game2048SessionState
        {
            Id = Guid.NewGuid(),
            GameSessionId = gameSession.Id,
            Size = size,
            BoardJson = JsonSerializer.Serialize(initialBoard),
            Score = 0,
            BestTile = GetBestTile(initialBoard),
            MoveCount = 0,
            CreationTime = DateTime.UtcNow
        };

        await _repository.CreateAsync(sessionState);
        return sessionState;
    }

    private static Game2048State ConvertToGameState(Game2048SessionState sessionState)
    {
        int[][] board = JsonSerializer.Deserialize<int[][]>(sessionState.BoardJson) ?? Array.Empty<int[]>();

        return new Game2048State
        {
            GameId = sessionState.GameSessionId,
            Players = sessionState.GameSession.Players.Select(p => p.ApplicationUserId).ToList(),
            IsFinished = sessionState.GameSession.IsFinished,
            Winner = sessionState.GameSession.WinnerId,
            Size = sessionState.Size,
            Board = board,
            Score = sessionState.Score,
            BestTile = sessionState.BestTile,
            MoveCount = sessionState.MoveCount,
            CreationTime = sessionState.CreationTime
        };
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
}
