using System.Collections.Immutable;
using Quizanchos.Common.Enums;
using Quizanchos.Core;
using Quizanchos.Quiz.GameLogic;
using Quizanchos.Quiz.Services;

namespace Quizanchos.QuizMultiplayer.GameLogic;

public class QuizMultiplayerGameLogic : IGameLogic<QuizMultiplayerGameState, QuizMultiplayerMove>
{
    private readonly int _totalCards;
    private readonly Guid _quizCategoryId;
    private readonly GameLevel _gameLevel;
    private readonly int _secondsPerCard;
    private readonly int _optionCount;
    private readonly List<QuizMultiplayerGameState.TeamData> _teams;
    private readonly QuizCardGeneratorService? _cardGenerator;

    public QuizMultiplayerGameLogic(
        int totalCards = 10,
        Guid? quizCategoryId = null,
        GameLevel gameLevel = GameLevel.Easy,
        int secondsPerCard = 30,
        int optionCount = 4,
        List<QuizMultiplayerGameState.TeamData>? teams = null,
        QuizCardGeneratorService? cardGenerator = null)
    {
        _totalCards = totalCards;
        _quizCategoryId = quizCategoryId ?? Guid.Empty;
        _gameLevel = gameLevel;
        _secondsPerCard = secondsPerCard;
        _optionCount = optionCount;
        _teams = teams ?? new List<QuizMultiplayerGameState.TeamData>();
        _cardGenerator = cardGenerator;
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

        // Generate next card lazily so each card gets its own CreationTime
        if (_cardGenerator != null
            && state.CurrentCardIndex < state.TotalCards
            && state.CurrentCardIndex >= state.Cards.Count
            && state.QuizCategoryId != Guid.Empty)
        {
            GenerateAndAppendSingleCard(state);
        }
        else if (state.CurrentCardIndex < state.Cards.Count)
        {
            // Backward compatibility for sessions where cards were pre-generated
            state.Cards[state.CurrentCardIndex].CreationTime = DateTime.UtcNow;
        }
    }

    private void GenerateAndAppendSingleCard(QuizMultiplayerGameState state)
    {
        QuizGameState tempState = new QuizGameState
        {
            GameId = state.GameId,
            Players = state.Players,
            TotalCards = state.TotalCards,
            CurrentCardIndex = state.CurrentCardIndex - 1,
            OptionCount = state.OptionCount,
            GameLevel = state.GameLevel,
            QuizCategoryId = state.QuizCategoryId,
            Cards = Enumerable.Range(0, state.Cards.Count)
                .Select(_ => new QuizGameState.QuizCard())
                .ToList()
        };

        _cardGenerator!
            .GenerateSingleCard(tempState, state.QuizCategoryId, state.OptionCount, state.GameLevel)
            .GetAwaiter()
            .GetResult();

        QuizGameState.QuizCard generatedCard = tempState.Cards.Last();
        state.Cards.Add(new QuizMultiplayerGameState.QuizMultiplayerCard
        {
            Id = generatedCard.Id,
            CardIndex = generatedCard.CardIndex,
            CorrectOption = generatedCard.CorrectOption,
            EntityIds = generatedCard.EntityIds,
            EntityNames = generatedCard.EntityNames,
            OptionValues = generatedCard.OptionValues,
            PlayerAnswers = new Dictionary<string, int?>(),
            CreationTime = generatedCard.CreationTime,
            TeamVotes = new Dictionary<int, int>()
        });
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

    public IReadOnlyDictionary<string, int> GetPlayerScores(QuizMultiplayerGameState state)
    {
        var scores = new Dictionary<string, int>();

        if (state.TeamScores.Count == 0)
            return scores;

        // Determine if there's a winner or draw
        int maxScore = state.TeamScores.Max(kvp => kvp.Value);
        var winners = state.TeamScores.Where(kvp => kvp.Value == maxScore).ToList();

        bool isDraw = winners.Count > 1;
        int? winningTeamIndex = isDraw ? null : winners[0].Key;

        // Award points to each player based on their team's result
        foreach (var team in state.Teams)
        {
            int points = 0;

            if (isDraw)
            {
                // All players in draw teams get 1 point
                points = 1;
            }
            else if (team.TeamIndex == winningTeamIndex)
            {
                // Winning team players get 3 points
                points = 3;
            }
            // Losing team players get 0 points (already initialized)

            foreach (var playerId in team.PlayerIds)
            {
                scores[playerId] = points;
            }
        }

        return scores;
    }
}
