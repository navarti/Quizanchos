using Microsoft.Extensions.Logging;
using Quizanchos.Common.Enums;
using Quizanchos.Core;
using Quizanchos.Quiz.GameLogic;
using Quizanchos.Quiz.Services;
using Quizanchos.QuizMultiplayer.GameLogic;
using System.Collections.Immutable;
using System.Text.Json;

namespace Quizanchos.QuizMultiplayer.Services;

public class QuizMultiplayerEngineFactory
{
    private const int MinigameTypeId = 3;

    private readonly ILogger<QuizMultiplayerEngineFactory> _logger;
    private readonly IGameStatePersistence _persistence;
    private readonly QuizCardGeneratorService? _cardGenerator;

    public QuizMultiplayerEngineFactory(
        ILogger<QuizMultiplayerEngineFactory> logger,
        IGameStatePersistence persistence,
        QuizCardGeneratorService? cardGenerator = null)
    {
        _logger = logger;
        _persistence = persistence;
        _cardGenerator = cardGenerator;
    }

    public async Task<GameEngine<QuizMultiplayerGameState, QuizMultiplayerMove>> CreateEngineAsync(
        Guid gameId,
        ImmutableArray<string> playerIds,
        int totalCards,
        Guid? categoryId,
        GameLevel gameLevel,
        int secondsPerCard,
        int optionCount,
        List<QuizMultiplayerGameState.TeamData> teams)
    {
        _logger.LogInformation(
            "Creating QuizMultiplayer engine: TotalCards={TotalCards}, CategoryId={CategoryId}, Teams={TeamCount}, Players={PlayerCount}",
            totalCards, categoryId, teams.Count, playerIds.Length);

        var logic = new QuizMultiplayerGameLogic(
            totalCards, categoryId, gameLevel, secondsPerCard, optionCount, teams, _cardGenerator);

        var engine = new GameEngine<QuizMultiplayerGameState, QuizMultiplayerMove>(logic, gameId, playerIds);
        var state = engine.State;

        // Generate only the first card. Next cards are generated lazily after each round.
        if (categoryId.HasValue && categoryId.Value != Guid.Empty && _cardGenerator != null)
        {
            _logger.LogInformation("Generating first card via QuizCardGeneratorService");
            try
            {
                var tempState = new QuizGameState
                {
                    GameId = gameId,
                    Players = playerIds.ToList(),
                    TotalCards = totalCards,
                    CurrentCardIndex = -1,
                    OptionCount = optionCount,
                    GameLevel = gameLevel
                };

                await _cardGenerator.GenerateSingleCard(tempState, categoryId.Value, optionCount, gameLevel);

                var quizCard = tempState.Cards.FirstOrDefault();
                if (quizCard != null)
                {
                    state.Cards.Add(new QuizMultiplayerGameState.QuizMultiplayerCard
                    {
                        Id = quizCard.Id,
                        CardIndex = quizCard.CardIndex,
                        CorrectOption = quizCard.CorrectOption,
                        EntityIds = quizCard.EntityIds,
                        EntityNames = quizCard.EntityNames,
                        OptionValues = quizCard.OptionValues,
                        PlayerAnswers = new Dictionary<string, int?>(),
                        CreationTime = quizCard.CreationTime,
                        TeamVotes = new Dictionary<int, int>()
                    });
                }

                state.CurrentCardIndex = 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating first card for QuizMultiplayer game {GameId}", gameId);
            }
        }
        else
        {
            _logger.LogWarning("No category ID provided or card generator unavailable, cards will not be generated");
        }

        await _persistence.CreateAsync(
            gameId,
            MinigameTypeId,
            playerIds,
            JsonSerializer.Serialize(state));

        _logger.LogInformation(
            "QuizMultiplayer engine created: GameId={GameId}, Cards={CardsCount}, CurrentCardIndex={CurrentCardIndex}",
            gameId, state.Cards.Count, state.CurrentCardIndex);

        return engine;
    }

    public async Task<GameEngine<QuizMultiplayerGameState, QuizMultiplayerMove>?> LoadEngineAsync(Guid gameId)
    {
        _logger.LogInformation("Loading QuizMultiplayer engine for GameId={GameId}", gameId);

        var loaded = await _persistence.LoadAsync(gameId);
        if (loaded is null)
        {
            _logger.LogWarning("QuizMultiplayer state not found for GameId={GameId}", gameId);
            return null;
        }

        var state = JsonSerializer.Deserialize<QuizMultiplayerGameState>(loaded.StateJson);
        if (state is null)
        {
            _logger.LogWarning("QuizMultiplayer state failed to deserialize for GameId={GameId}", gameId);
            return null;
        }

        state.GameId = gameId;
        state.Players = loaded.PlayerIds;
        state.IsFinished = loaded.IsFinished;
        // state.Winner is the winning team name, preserved from JSON; do not overwrite with
        // loaded.Winner (which would be GameSession.WinnerId — left null for team-based games).

        var logic = new QuizMultiplayerGameLogic(
            state.TotalCards,
            state.QuizCategoryId,
            state.GameLevel,
            state.SecondsPerCard,
            state.OptionCount,
            state.Teams,
            _cardGenerator);

        var engine = new GameEngine<QuizMultiplayerGameState, QuizMultiplayerMove>(logic, state);

        _logger.LogInformation(
            "QuizMultiplayer engine loaded: CurrentCardIndex={CurrentCardIndex}, TotalCards={TotalCards}, Cards={CardsCount}",
            state.CurrentCardIndex, state.TotalCards, state.Cards.Count);

        return engine;
    }

    public async Task SaveStateAsync(Guid gameId, QuizMultiplayerGameState state)
    {
        await _persistence.UpdateAsync(gameId, JsonSerializer.Serialize(state));
    }
}
