using Microsoft.Extensions.DependencyInjection;
using Quizanchos.Common.Enums;
using Quizanchos.Core;
using Quizanchos.Quiz.Extensions;
using Quizanchos.Quiz.GameLogic;
using Quizanchos.Quiz.Services;
using System.Collections.Immutable;
using System.Text.Json;

namespace Quizanchos.Quiz.Descriptors;

/// <summary>
/// Descriptor for the Quiz minigame plugin.
/// Handles registration and lifecycle management of Quiz game instances.
/// </summary>
public class QuizMinigameDescriptor : IMinigameDescriptor
{
    public int MinigameTypeId => 1;
    public string GameKey => "Quiz";
    public string DisplayName => "Quiz";
    public bool IsPremium => false;
    public Type MoveType => typeof(QuizMove);
    public string MoveDiscriminator => "quiz";

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
        int totalCards = GetInt(parameters, "totalCards", 5);
        Guid? categoryId = GetNullableGuid(parameters, "categoryId");
        GameLevel gameLevel = GetEnum(parameters, "gameLevel", GameLevel.Easy);
        int secondsPerCard = GetInt(parameters, "secondsPerCard", 10);
        int optionCount = GetInt(parameters, "optionCount", 4);

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

    private static int GetInt(Dictionary<string, object> parameters, string key, int fallback)
    {
        if (!parameters.TryGetValue(key, out var value) || value == null)
            return fallback;

        return value switch
        {
            int intValue => intValue,
            long longValue => (int)longValue,
            string stringValue when int.TryParse(stringValue, out var parsed) => parsed,
            JsonElement { ValueKind: JsonValueKind.Number } element when element.TryGetInt32(out var parsed) => parsed,
            JsonElement { ValueKind: JsonValueKind.String } element when int.TryParse(element.GetString(), out var parsed) => parsed,
            _ => fallback
        };
    }

    private static Guid? GetNullableGuid(Dictionary<string, object> parameters, string key)
    {
        if (!parameters.TryGetValue(key, out var value) || value == null)
            return null;

        return value switch
        {
            Guid guid => guid,
            string stringValue when Guid.TryParse(stringValue, out var parsed) => parsed,
            JsonElement { ValueKind: JsonValueKind.String } element when Guid.TryParse(element.GetString(), out var parsed) => parsed,
            _ => null
        };
    }

    private static TEnum GetEnum<TEnum>(Dictionary<string, object> parameters, string key, TEnum fallback)
        where TEnum : struct, Enum
    {
        if (!parameters.TryGetValue(key, out var value) || value == null)
            return fallback;

        return value switch
        {
            TEnum enumValue => enumValue,
            int intValue when Enum.IsDefined(typeof(TEnum), intValue) => (TEnum)Enum.ToObject(typeof(TEnum), intValue),
            long longValue when Enum.IsDefined(typeof(TEnum), (int)longValue) => (TEnum)Enum.ToObject(typeof(TEnum), (int)longValue),
            string stringValue when Enum.TryParse<TEnum>(stringValue, true, out var parsed) => parsed,
            JsonElement { ValueKind: JsonValueKind.Number } element when element.TryGetInt32(out var parsedInt) && Enum.IsDefined(typeof(TEnum), parsedInt)
                => (TEnum)Enum.ToObject(typeof(TEnum), parsedInt),
            JsonElement { ValueKind: JsonValueKind.String } element when Enum.TryParse<TEnum>(element.GetString(), true, out var parsed)
                => parsed,
            _ => fallback
        };
    }
}
