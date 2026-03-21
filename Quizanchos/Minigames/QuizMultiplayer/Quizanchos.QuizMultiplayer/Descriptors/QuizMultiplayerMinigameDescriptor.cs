using Microsoft.Extensions.DependencyInjection;
using Quizanchos.Common.Enums;
using Quizanchos.Core;
using Quizanchos.Quiz.GameLogic;
using Quizanchos.Quiz.Services;
using Quizanchos.QuizMultiplayer.Extensions;
using Quizanchos.QuizMultiplayer.GameLogic;
using Quizanchos.QuizMultiplayer.Services;
using System.Collections.Immutable;

namespace Quizanchos.QuizMultiplayer.Descriptors;

/// <summary>
/// Descriptor for the QuizMultiplayer minigame plugin.
/// Handles registration and lifecycle management of QuizMultiplayer game instances.
/// </summary>
public class QuizMultiplayerMinigameDescriptor : IMinigameDescriptor
{
    public int MinigameTypeId => 3;
    public string GameKey => "QuizMultiplayer";
    public string DisplayName => "Quiz Multiplayer";

    public void RegisterServices(IServiceCollection services)
    {
        // Register all QuizMultiplayer-specific repositories and services
        services.AddQuizMultiplayerRepositories();
        services.AddQuizMultiplayerServices();

        // Also register Quiz services for card generation
        // (These may already be registered, but we ensure they exist)
        services.AddScoped<QuizCardGeneratorService>();
    }

    public async Task<IGameEngine> CreateGameEngineAsync(Guid gameId, ImmutableArray<string> playerIds,
        Dictionary<string, object> parameters, IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetRequiredService<QuizMultiplayerEngineFactory>();

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

        // Extract teams if provided, otherwise create empty list
        List<QuizMultiplayerGameState.TeamData> teams = new();
        if (parameters.ContainsKey("teams") && parameters["teams"] is List<QuizMultiplayerGameState.TeamData> teamsList)
        {
            teams = teamsList;
        }

        // Create the game engine
        var engine = await factory.CreateEngineAsync(
            gameId,
            playerIds,
            totalCards,
            categoryId,
            gameLevel,
            secondsPerCard,
            optionCount,
            teams);

        // Wrap the engine to comply with IGameEngine interface
        return new GameEngineWrapper<QuizMultiplayerGameState, QuizMultiplayerMove>(engine);
    }

    public async Task<IGameEngine?> LoadGameEngineAsync(Guid gameId, IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetRequiredService<QuizMultiplayerEngineFactory>();

        var engine = await factory.LoadEngineAsync(gameId);
        if (engine == null)
            return null;

        return new GameEngineWrapper<QuizMultiplayerGameState, QuizMultiplayerMove>(engine);
    }

    public async Task SaveGameStateAsync(Guid gameId, IGameState state, IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetRequiredService<QuizMultiplayerEngineFactory>();

        if (state is QuizMultiplayerGameState quizMultiplayerState)
        {
            await factory.SaveStateAsync(gameId, quizMultiplayerState);
        }
    }
}
