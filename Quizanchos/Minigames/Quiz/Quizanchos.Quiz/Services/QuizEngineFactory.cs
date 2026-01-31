using Microsoft.Extensions.Logging;
using Quizanchos.Common.Enums;
using Quizanchos.Core;
using Quizanchos.Quiz.GameLogic;
using System.Collections.Immutable;

namespace Quizanchos.Quiz.Services;

public class QuizEngineFactory
{
    private readonly ILogger<QuizEngineFactory> _logger;
    private readonly QuizCardGeneratorService? _cardGenerator;

    public QuizEngineFactory(
        ILogger<QuizEngineFactory> logger,
        QuizCardGeneratorService? cardGenerator = null)
    {
        _logger = logger;
        _cardGenerator = cardGenerator;
    }

    public GameEngine<QuizGameState, QuizMove> CreateQuizEngine(
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
            
            Task.Run(async () =>
            {
                try
                {
                    await _cardGenerator.GenerateCardsForGame(state, categoryId.Value, totalCards, optionCount, gameLevel);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating cards for game {GameId}", gameId);
                }
            }).GetAwaiter().GetResult();
        }
        else
        {
            _logger.LogWarning("No category ID provided or card generator unavailable, cards will not be generated");
        }
        
        return engine;
    }
}
