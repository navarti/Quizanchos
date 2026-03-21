using Microsoft.Extensions.DependencyInjection;
using Quizanchos.Common.Enums;
using Quizanchos.Core;
using Quizanchos.Quiz.Extensions;
using Quizanchos.Quiz.GameLogic;
using Quizanchos.Quiz.Services;
using System.Collections.Immutable;

namespace Quizanchos.Quiz.Descriptors;

/// <summary>
/// Descriptor for the Quiz minigame plugin.
/// Handles registration and lifecycle management of Quiz game instances.
/// </summary>
public class QuizMinigameDescriptor : IMinigameDescriptor
{
    public string GameKey => nameof(MinigameType.Quiz);
    public string DisplayName => "Quiz";

    public void RegisterServices(IServiceCollection services)
    {
        // Register all Quiz-specific repositories and services
        services.AddQuizRepositories();
        services.AddQuizServices();
    }

    public async Task<IGameEngine> CreateGameEngineAsync(Guid gameId, ImmutableArray<string> playerIds,
        Dictionary<string, object> parameters, IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetRequiredService<QuizEngineFactory>();

        // Extract game parameters with defaults
        int totalCards = parameters.ContainsKey("totalCards") 
            ? (int)parameters["totalCards"] 
            : 5;

        Guid? categoryId = parameters.ContainsKey("categoryId")
            ? (Guid)parameters["categoryId"]
            : null;

        GameLevel gameLevel = parameters.ContainsKey("gameLevel")
            ? (GameLevel)parameters["gameLevel"]
            : GameLevel.Easy;

        int secondsPerCard = parameters.ContainsKey("secondsPerCard")
            ? (int)parameters["secondsPerCard"]
            : 10;

        int optionCount = parameters.ContainsKey("optionCount")
            ? (int)parameters["optionCount"]
            : 4;

        // Create the game engine
        var engine = await factory.CreateQuizEngineAsync(
            gameId,
            playerIds,
            totalCards,
            categoryId,
            gameLevel,
            secondsPerCard,
            optionCount);

        // Wrap the engine to comply with IGameEngine interface
        return new GameEngineWrapper<QuizGameState, QuizMove>(engine);
    }

    public async Task<IGameEngine?> LoadGameEngineAsync(Guid gameId, IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetRequiredService<QuizEngineFactory>();

        var engine = await factory.LoadQuizEngineAsync(gameId);
        if (engine == null)
            return null;

        return new GameEngineWrapper<QuizGameState, QuizMove>(engine);
    }

    public async Task SaveGameStateAsync(Guid gameId, IGameState state, IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetRequiredService<QuizEngineFactory>();

        if (state is QuizGameState quizState)
        {
            await factory.SaveQuizStateAsync(gameId, quizState);
        }
    }
}
