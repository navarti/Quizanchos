using System.Collections.Immutable;
using Quizanchos.Common.Enums;
using Quizanchos.Core;

namespace Quizanchos.QuizMultiplayer.GameLogic;

public class QuizMultiplayerGameLogic : IGameLogic<QuizMultiplayerGameState, QuizMultiplayerMove>
{
    private readonly int _totalCards;
    private readonly Guid _quizCategoryId;
    private readonly GameLevel _gameLevel;
    private readonly int _secondsPerCard;
    private readonly int _optionCount;
    private readonly List<QuizMultiplayerGameState.TeamData> _teams;

    public QuizMultiplayerGameLogic(
        int totalCards = 10,
        Guid? quizCategoryId = null,
        GameLevel gameLevel = GameLevel.Easy,
        int secondsPerCard = 30,
        int optionCount = 4,
        List<QuizMultiplayerGameState.TeamData>? teams = null)
    {
        _totalCards = totalCards;
        _quizCategoryId = quizCategoryId ?? Guid.Empty;
        _gameLevel = gameLevel;
        _secondsPerCard = secondsPerCard;
        _optionCount = optionCount;
        _teams = teams ?? new List<QuizMultiplayerGameState.TeamData>();
    }

    public QuizMultiplayerGameState CreateInitialState(Guid gameId, ImmutableArray<string> players)
    {
        var state = new QuizMultiplayerGameState
        {
            GameId = gameId,
            Players = players.ToList(),
            IsFinished = false,
            Winner = null,
            CurrentCardIndex = -1,
            TotalCards = _totalCards,
            QuizCategoryId = _quizCategoryId,
            GameLevel = _gameLevel,
            SecondsPerCard = _secondsPerCard,
            OptionCount = _optionCount,
            CreationTime = DateTime.UtcNow,
            IsTerminatedByTime = false,
            Teams = _teams.Select(t => new QuizMultiplayerGameState.TeamData
            {
                TeamIndex = t.TeamIndex,
                Name = t.Name,
                PlayerIds = t.PlayerIds.ToList()
            }).ToList()
        };

        foreach (var team in state.Teams)
        {
            state.TeamScores[team.TeamIndex] = 0;
        }

        return state;
    }

    public MoveResult ValidateMove(QuizMultiplayerGameState state, QuizMultiplayerMove move, string playerId)
    {
        if (state.CurrentCardIndex < 0 || state.CurrentCardIndex >= state.Cards.Count)
        {
            return MoveResult.Failure("No current card available");
        }

        var currentCard = state.Cards[state.CurrentCardIndex];

        if (currentCard.PlayerAnswers.ContainsKey(playerId))
        {
            return MoveResult.Failure("Player has already answered this card");
        }

        if (move.OptionPicked < 0)
        {
            return MoveResult.Failure("Invalid option picked");
        }

        return MoveResult.Success;
    }

    public void ApplyMove(QuizMultiplayerGameState state, QuizMultiplayerMove move, string playerId)
    {
        var currentCard = state.Cards[state.CurrentCardIndex];
        currentCard.PlayerAnswers[playerId] = move.OptionPicked;

        // Check if all players have answered
        if (currentCard.PlayerAnswers.Count < state.Players.Count)
            return;

        // All players answered — tally team votes
        foreach (var team in state.Teams)
        {
            var teamVotes = team.PlayerIds
                .Where(p => currentCard.PlayerAnswers.ContainsKey(p) && currentCard.PlayerAnswers[p].HasValue)
                .Select(p => currentCard.PlayerAnswers[p]!.Value)
                .GroupBy(v => v)
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key)
                .FirstOrDefault();

            int teamAnswer = teamVotes?.Key ?? -1;
            currentCard.TeamVotes[team.TeamIndex] = teamAnswer;

            if (teamAnswer == currentCard.CorrectOption)
            {
                state.TeamScores[team.TeamIndex]++;
            }
        }

        state.CurrentCardIndex++;

        // Reset the new card's creation time so the per-card timer starts fresh
        if (state.CurrentCardIndex < state.Cards.Count)
        {
            state.Cards[state.CurrentCardIndex].CreationTime = DateTime.UtcNow;
        }
    }

    public bool CheckFinished(QuizMultiplayerGameState state)
    {
        return state.CurrentCardIndex >= state.TotalCards;
    }

    public string? DetermineWinner(QuizMultiplayerGameState state)
    {
        if (state.TeamScores.Count == 0)
            return null;

        int maxScore = state.TeamScores.Max(kvp => kvp.Value);
        var winners = state.TeamScores.Where(kvp => kvp.Value == maxScore).ToList();

        if (winners.Count > 1)
            return null; // Draw

        int winningTeamIndex = winners[0].Key;
        var winningTeam = state.Teams.FirstOrDefault(t => t.TeamIndex == winningTeamIndex);
        return winningTeam?.Name;
    }

    public IEnumerable<string> GetExpectedPlayers(QuizMultiplayerGameState state)
    {
        if (state.CurrentCardIndex < 0 || state.CurrentCardIndex >= state.Cards.Count)
        {
            return Enumerable.Empty<string>();
        }

        var currentCard = state.Cards[state.CurrentCardIndex];
        return state.Players.Where(p => !currentCard.PlayerAnswers.ContainsKey(p));
    }

    public bool NeedToFinish(QuizMultiplayerGameState state)
    {
        if (state.CurrentCardIndex < 0 || state.CurrentCardIndex >= state.Cards.Count)
        {
            return false;
        }

        var currentCard = state.Cards[state.CurrentCardIndex];
        var expirationTime = currentCard.CreationTime.AddSeconds(_secondsPerCard);
        return DateTime.UtcNow > expirationTime;
    }
}
