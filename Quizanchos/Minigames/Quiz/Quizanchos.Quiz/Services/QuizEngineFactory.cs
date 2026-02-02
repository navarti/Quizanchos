using Microsoft.Extensions.Logging;
using Quizanchos.Common.Enums;
using Quizanchos.Core;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.Quiz.GameLogic;
using System.Collections.Immutable;

namespace Quizanchos.Quiz.Services;

public class QuizEngineFactory
{
    private readonly ILogger<QuizEngineFactory> _logger;
    private readonly QuizCardGeneratorService? _cardGenerator;
    private readonly QuizGameStateService _stateService;
    private readonly IGameSessionRepository _gameSessionRepository;

    public QuizEngineFactory(
        ILogger<QuizEngineFactory> logger,
        QuizGameStateService stateService,
        IGameSessionRepository gameSessionRepository,
        QuizCardGeneratorService? cardGenerator = null)
    {
        _logger = logger;
        _stateService = stateService;
        _gameSessionRepository = gameSessionRepository;
        _cardGenerator = cardGenerator;
    }

    public async Task<GameEngine<QuizGameState, QuizMove>> CreateQuizEngineAsync(
        Guid gameId, 
        ImmutableArray<Guid> playerIds,
        int totalCards,
        Guid? categoryId,
        GameLevel gameLevel,
        int secondsPerCard,
        int optionCount)
    {
        _logger.LogInformation("Creating Quiz engine with: TotalCards={TotalCards}, CategoryId={CategoryId}, GameLevel={GameLevel}, SecondsPerCard={SecondsPerCard}, OptionCount={OptionCount}",
            totalCards, categoryId, gameLevel, secondsPerCard, optionCount);

        // Create GameSession first
        GameSession gameSession = new GameSession
        {
            Id = gameId,
            MinigameType = MinigameType.Quiz,
            IsActive = true,
            IsFinished = false,
            CreatedAt = DateTime.UtcNow
        };

        foreach (Guid playerId in playerIds)
        {
            gameSession.Players.Add(new GameSessionPlayer
            {
                Id = Guid.NewGuid(),
                GameSessionId = gameId,
                ApplicationUserId = playerId.ToString(),
                JoinedAt = DateTime.UtcNow
            });
        }

        await _gameSessionRepository.CreateAsync(gameSession);

        // Create quiz-specific state
        await _stateService.CreateInitialStateAsync(
            gameSession,
            categoryId ?? Guid.Empty,
            totalCards,
            (int)gameLevel,
            secondsPerCard,
            optionCount);

        // Create the engine
        QuizGameLogic logic = new QuizGameLogic(
            totalCards,
            categoryId,
            gameLevel,
            secondsPerCard,
            optionCount,
            _cardGenerator
        );
        
        GameEngine<QuizGameState, QuizMove> engine = new GameEngine<QuizGameState, QuizMove>(logic, gameId, playerIds);
        
        QuizGameState state = engine.State;
        _logger.LogInformation("Quiz engine created. Initial state: CurrentCardIndex={CurrentCardIndex}, TotalCards={TotalCards}, Cards.Count={CardsCount}",
            state.CurrentCardIndex, state.TotalCards, state.Cards.Count);

        // Generate cards if categoryId is provided
        if (categoryId.HasValue && categoryId.Value != Guid.Empty && _cardGenerator != null)
        {
            _logger.LogInformation("Delegating card generation to QuizCardGeneratorService");
            
            try
            {
                await _cardGenerator.GenerateCardsForGame(state, categoryId.Value, totalCards, optionCount, gameLevel);
                // Save generated cards to DB
                await _stateService.SaveStateAsync(gameId, state);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating cards for game {GameId}", gameId);
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

        QuizGameState? state = await _stateService.LoadStateAsync(gameId);
        if (state == null)
        {
            _logger.LogWarning("Quiz state not found for GameId={GameId}", gameId);
            return null;
        }

        QuizGameLogic logic = new QuizGameLogic(
            state.TotalCards,
            state.QuizCategoryId,
            state.GameLevel,
            state.SecondsPerCard,
            state.OptionCount,
            _cardGenerator
        );

        GameEngine<QuizGameState, QuizMove> engine = new GameEngine<QuizGameState, QuizMove>(
            logic, 
            state);

        _logger.LogInformation("Quiz engine loaded. State: CurrentCardIndex={CurrentCardIndex}, TotalCards={TotalCards}, Cards.Count={CardsCount}",
            state.CurrentCardIndex, state.TotalCards, state.Cards.Count);

        return engine;
    }

    public async Task SaveQuizStateAsync(Guid gameId, QuizGameState state)
    {
        await _stateService.SaveStateAsync(gameId, state);
    }
}
