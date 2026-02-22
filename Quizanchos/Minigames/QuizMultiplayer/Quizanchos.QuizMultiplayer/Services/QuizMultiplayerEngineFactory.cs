using Microsoft.Extensions.Logging;
using Quizanchos.Common.Enums;
using Quizanchos.Core;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.Quiz.GameLogic;
using Quizanchos.Quiz.Services;
using Quizanchos.QuizMultiplayer.GameLogic;
using System.Collections.Immutable;

namespace Quizanchos.QuizMultiplayer.Services;

public class QuizMultiplayerEngineFactory
{
    private readonly ILogger<QuizMultiplayerEngineFactory> _logger;
    private readonly QuizMultiplayerStateService _stateService;
    private readonly IGameSessionRepository _gameSessionRepository;
    private readonly QuizCardGeneratorService? _cardGenerator;

    public QuizMultiplayerEngineFactory(
        ILogger<QuizMultiplayerEngineFactory> logger,
        QuizMultiplayerStateService stateService,
        IGameSessionRepository gameSessionRepository,
        QuizCardGeneratorService? cardGenerator = null)
    {
        _logger = logger;
        _stateService = stateService;
        _gameSessionRepository = gameSessionRepository;
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

        // Create GameSession
        GameSession gameSession = new GameSession
        {
            Id = gameId,
            MinigameType = MinigameType.QuizMultiplayer,
            IsActive = true,
            IsFinished = false,
            CreatedAt = DateTime.UtcNow
        };

        foreach (string playerId in playerIds)
        {
            gameSession.Players.Add(new GameSessionPlayer
            {
                Id = Guid.NewGuid(),
                GameSessionId = gameId,
                ApplicationUserId = playerId,
                JoinedAt = DateTime.UtcNow
            });
        }

        await _gameSessionRepository.CreateAsync(gameSession);

        // Create the engine
        QuizMultiplayerGameLogic logic = new QuizMultiplayerGameLogic(
            totalCards, categoryId, gameLevel, secondsPerCard, optionCount, teams);

        GameEngine<QuizMultiplayerGameState, QuizMultiplayerMove> engine =
            new GameEngine<QuizMultiplayerGameState, QuizMultiplayerMove>(logic, gameId, playerIds);

        QuizMultiplayerGameState state = engine.State;

        // Generate cards by reusing Quiz's card generator with a temporary QuizGameState
        if (categoryId.HasValue && categoryId.Value != Guid.Empty && _cardGenerator != null)
        {
            _logger.LogInformation("Generating cards via QuizCardGeneratorService");
            try
            {
                QuizGameState tempState = new QuizGameState
                {
                    GameId = gameId,
                    Players = playerIds.ToList(),
                    TotalCards = totalCards,
                    CurrentCardIndex = -1,
                    OptionCount = optionCount,
                    GameLevel = gameLevel
                };

                await _cardGenerator.GenerateCardsForGame(tempState, categoryId.Value, totalCards, optionCount, gameLevel);

                // Copy generated cards into multiplayer state
                foreach (var quizCard in tempState.Cards)
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
                _logger.LogError(ex, "Error generating cards for QuizMultiplayer game {GameId}", gameId);
            }
        }
        else
        {
            _logger.LogWarning("No category ID provided or card generator unavailable, cards will not be generated");
        }

        // Persist initial state
        await _stateService.CreateInitialStateAsync(gameId, state);

        _logger.LogInformation(
            "QuizMultiplayer engine created: GameId={GameId}, Cards={CardsCount}, CurrentCardIndex={CurrentCardIndex}",
            gameId, state.Cards.Count, state.CurrentCardIndex);

        return engine;
    }

    public async Task<GameEngine<QuizMultiplayerGameState, QuizMultiplayerMove>?> LoadEngineAsync(Guid gameId)
    {
        _logger.LogInformation("Loading QuizMultiplayer engine for GameId={GameId}", gameId);

        QuizMultiplayerGameState? state = await _stateService.LoadStateAsync(gameId);
        if (state == null)
        {
            _logger.LogWarning("QuizMultiplayer state not found for GameId={GameId}", gameId);
            return null;
        }

        QuizMultiplayerGameLogic logic = new QuizMultiplayerGameLogic(
            state.TotalCards,
            state.QuizCategoryId,
            state.GameLevel,
            state.SecondsPerCard,
            state.OptionCount,
            state.Teams);

        GameEngine<QuizMultiplayerGameState, QuizMultiplayerMove> engine =
            new GameEngine<QuizMultiplayerGameState, QuizMultiplayerMove>(logic, state);

        _logger.LogInformation(
            "QuizMultiplayer engine loaded: CurrentCardIndex={CurrentCardIndex}, TotalCards={TotalCards}, Cards={CardsCount}",
            state.CurrentCardIndex, state.TotalCards, state.Cards.Count);

        return engine;
    }

    public async Task SaveStateAsync(Guid gameId, QuizMultiplayerGameState state)
    {
        await _stateService.SaveStateAsync(gameId, state);
    }
}
