using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;

namespace Quizanchos.WebApi.Services.Users;

/// <summary>
/// Service for managing user scores per minigame type.
/// Follows repository pattern for CRUD operations with UserMinigameScore entity.
/// </summary>
public class UserScoreService
{
    private readonly IUserMinigameScoreRepository _scoreRepository;

    public UserScoreService(IUserMinigameScoreRepository scoreRepository)
    {
        _scoreRepository = scoreRepository;
    }

    /// <summary>
    /// Increment or set user's score for a specific minigame type.
    /// </summary>
    public async Task IncrementScoreAsync(string userId, int minigameType, int points)
    {
        if (points <= 0)
            return;

        var existingScore = await _scoreRepository.FindByUserAndTypeAsync(userId, minigameType).ConfigureAwait(false);

        if (existingScore != null)
        {
            // Update existing score
            existingScore.Score += points;
            await _scoreRepository.Update(existingScore).ConfigureAwait(false);
        }
        else
        {
            // Create new score record
            var newScore = new UserMinigameScore
            {
                Id = Guid.NewGuid(),
                ApplicationUserId = userId,
                MinigameType = minigameType,
                Score = points
            };
            await _scoreRepository.Create(newScore).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Get total score for a user across all minigames.
    /// </summary>
    public async Task<int> GetTotalScoreAsync(string userId)
    {
        var scores = await _scoreRepository.GetByFilter(s => s.ApplicationUserId == userId).ConfigureAwait(false);
        return scores.Sum(s => s.Score);
    }

    /// <summary>
    /// Get score for a specific minigame.
    /// </summary>
    public async Task<int> GetScoreAsync(string userId, int minigameType)
    {
        var score = await _scoreRepository.FindByUserAndTypeAsync(userId, minigameType).ConfigureAwait(false);
        return score?.Score ?? 0;
    }
}
