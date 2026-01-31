using System.Collections.Immutable;
using Quizanchos.Common.Enums;
using Quizanchos.Core;
using Quizanchos.Quiz.Services;

namespace Quizanchos.Quiz.GameLogic;

public class QuizGameLogic : IGameLogic<QuizGameState, QuizMove>
{
    private readonly int _totalCards;
    private readonly Guid _quizCategoryId;
    private readonly GameLevel _gameLevel;
    private readonly int _secondsPerCard;
    private readonly int _optionCount;
    private readonly QuizCardGeneratorService? _cardGenerator;

    public QuizGameLogic(
        int totalCards = 10,
        Guid? quizCategoryId = null,
        GameLevel gameLevel = GameLevel.Easy,
        int secondsPerCard = 30,
        int optionCount = 4,
        QuizCardGeneratorService? cardGenerator = null)
    {
        _totalCards = totalCards;
        _quizCategoryId = quizCategoryId ?? Guid.Empty;
        _gameLevel = gameLevel;
        _secondsPerCard = secondsPerCard;
        _optionCount = optionCount;
        _cardGenerator = cardGenerator;
    }

    public QuizGameState CreateInitialState(Guid gameId, ImmutableArray<Guid> players)
    {
        QuizGameState state = new QuizGameState
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
            IsTerminatedByTime = false
        };

        foreach (Guid playerId in players)
        {
            state.PlayerScores[playerId] = 0;
        }

        return state;
    }

    public MoveResult ValidateMove(QuizGameState state, QuizMove move, Guid playerId)
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

    public void ApplyMove(QuizGameState state, QuizMove move, Guid playerId)
    {
        var currentCard = state.Cards[state.CurrentCardIndex];
        currentCard.PlayerAnswers[playerId] = move.OptionPicked;
        currentCard.OptionPicked = move.OptionPicked;

        if (move.OptionPicked == currentCard.CorrectOption)
        {
            state.PlayerScores[playerId]++;
        }

        // After answering, move to the next card (if not the last card)
        if (state.CurrentCardIndex < state.TotalCards - 1)
        {
            state.CurrentCardIndex++;
            
            // Generate the next card if it doesn't exist yet and we have a card generator
            if (_cardGenerator != null && state.CurrentCardIndex >= state.Cards.Count)
            {
                Task.Run(async () =>
                {
                    await _cardGenerator.GenerateSingleCard(
                        state,
                        state.QuizCategoryId,
                        state.OptionCount,
                        state.GameLevel
                    );
                }).GetAwaiter().GetResult();
            }
        }
    }

    public bool CheckFinished(QuizGameState state)
    {
        return state.CurrentCardIndex >= state.TotalCards - 1;
    }

    public Guid? DetermineWinner(QuizGameState state)
    {
        if (state.PlayerScores.Count == 0)
            return null;

        var maxScore = state.PlayerScores.Max(kvp => kvp.Value);
        var winners = state.PlayerScores.Where(kvp => kvp.Value == maxScore).ToList();

        if (winners.Count > 1)
            return null; // Draw

        return winners[0].Key;
    }

    public IEnumerable<Guid> GetExpectedPlayers(QuizGameState state)
    {
        if (state.CurrentCardIndex < 0 || state.CurrentCardIndex >= state.Cards.Count)
        {
            return Enumerable.Empty<Guid>();
        }

        var currentCard = state.Cards[state.CurrentCardIndex];
        return state.Players.Where(p => !currentCard.PlayerAnswers.ContainsKey(p));
    }

    public void AddCard(
        QuizGameState state,
        Guid cardId,
        int correctOption,
        Guid[] entityIds,
        string[] entityNames,
        object[] optionValues)
    {
        state.CurrentCardIndex++;
        state.Cards.Add(new QuizGameState.QuizCard
        {
            Id = cardId,
            CardIndex = state.CurrentCardIndex,
            CorrectOption = correctOption,
            EntityIds = entityIds,
            EntityNames = entityNames,
            OptionValues = optionValues,
            PlayerAnswers = new Dictionary<Guid, int?>(),
            CreationTime = DateTime.UtcNow
        });
    }
}
