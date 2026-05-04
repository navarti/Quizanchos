using Microsoft.Extensions.Logging;
using Quizanchos.Common.Enums;
using Quizanchos.Core;
using Quizanchos.Quiz.GameLogic;
using System.Collections.Immutable;
using System.Text.Json;

namespace Quizanchos.Quiz.Services;

public class QuizEngineFactory
{
    private const int MinigameTypeId = 1;

    private readonly ILogger<QuizEngineFactory> _logger;
    private readonly IGameStatePersistence _persistence;
    private readonly QuizCardGeneratorService? _cardGenerator;

    public QuizEngineFactory(
        ILogger<QuizEngineFactory> logger,
        IGameStatePersistence persistence,
        QuizCardGeneratorService? cardGenerator = null)
    {
        _logger = logger;
        _persistence = persistence;
        _cardGenerator = cardGenerator;
    }

    public async Task<GameEngine<QuizGameState, QuizMove>> CreateQuizEngineAsync(
        Guid gameId,
        ImmutableArray<string> playerIds,
        int totalCards,
        Guid? categoryId,
        GameLevel gameLevel,
        int secondsPerCard,
        int optionCount)
    {
        _logger.LogInformation(
            "Creating Quiz engine with: TotalCards={TotalCards}, CategoryId={CategoryId}, GameLevel={GameLevel}, SecondsPerCard={SecondsPerCard}, OptionCount={OptionCount}",
            totalCards, categoryId, gameLevel, secondsPerCard, optionCount);

        var logic = new QuizGameLogic(
            totalCards,
            categoryId,
            gameLevel,
            secondsPerCard,
            optionCount,
            _cardGenerator);

        var engine = new GameEngine<QuizGameState, QuizMove>(logic, gameId, playerIds);
        var state = engine.State;

        await _persistence.CreateAsync(
            gameId,
            MinigameTypeId,
            playerIds,
            JsonSerializer.Serialize(state));

        _logger.LogInformation(
            "Quiz engine created. Initial state: CurrentCardIndex={CurrentCardIndex}, TotalCards={TotalCards}, Cards.Count={CardsCount}",
            state.CurrentCardIndex, state.TotalCards, state.Cards.Count);

        // Generate only the first card if categoryId is provided.
        // Next cards will be generated lazily after each move.
        if (categoryId.HasValue && categoryId.Value != Guid.Empty && _cardGenerator != null)
        {
            _logger.LogInformation("Delegating first-card generation to QuizCardGeneratorService");

            try
            {
                await _cardGenerator.GenerateSingleCard(state, categoryId.Value, optionCount, gameLevel);
                state.CurrentCardIndex = 0;

                await SaveQuizStateAsync(gameId, state);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating first card for game {GameId}", gameId);
            }
        }
        else
        {
            _logger.LogWarning("No category ID provided or card generator unavailable, cards will not be generated");
        }

        return engine;
    }

    public async Task<GameEngine<QuizGameState, QuizMove>?> LoadQuizEngineAsync(Guid gameId)
    {
        _logger.LogInformation("Loading Quiz engine for GameId={GameId}", gameId);

        var loaded = await _persistence.LoadAsync(gameId);
        if (loaded is null)
        {
            _logger.LogWarning("Quiz state not found for GameId={GameId}", gameId);
            return null;
        }

        var state = JsonSerializer.Deserialize<QuizGameState>(loaded.StateJson);
        if (state is null)
        {
            _logger.LogWarning("Quiz state failed to deserialize for GameId={GameId}", gameId);
            return null;
        }

        state.GameId = gameId;
        state.Players = loaded.PlayerIds;
        state.IsFinished = loaded.IsFinished;
        state.Winner = loaded.Winner;

        var logic = new QuizGameLogic(
            state.TotalCards,
            state.QuizCategoryId,
            state.GameLevel,
            state.SecondsPerCard,
            state.OptionCount,
            _cardGenerator);

        var engine = new GameEngine<QuizGameState, QuizMove>(logic, state);

        _logger.LogInformation(
            "Quiz engine loaded. State: CurrentCardIndex={CurrentCardIndex}, TotalCards={TotalCards}, Cards.Count={CardsCount}",
            state.CurrentCardIndex, state.TotalCards, state.Cards.Count);

        return engine;
    }

    public async Task SaveQuizStateAsync(Guid gameId, QuizGameState state)
    {
        await _persistence.UpdateAsync(gameId, JsonSerializer.Serialize(state));
    }
}
