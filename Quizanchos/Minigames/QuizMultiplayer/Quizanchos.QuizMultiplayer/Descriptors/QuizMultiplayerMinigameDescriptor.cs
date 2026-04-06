using Microsoft.Extensions.DependencyInjection;
using Quizanchos.Common.Enums;
using Quizanchos.Core;
using Quizanchos.Quiz.GameLogic;
using Quizanchos.Quiz.Services;
using Quizanchos.QuizMultiplayer.Extensions;
using Quizanchos.QuizMultiplayer.GameLogic;
using Quizanchos.QuizMultiplayer.Services;
using System.Collections.Immutable;
using System.Text.Json;

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
    public bool IsPremium => false;
    public Type MoveType => typeof(QuizMultiplayerMove);
    public string MoveDiscriminator => "quizMultiplayer";

    public void RegisterServices(IServiceCollection services)
    {
        // Register all QuizMultiplayer-specific repositories and services
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
        int totalCards = GetInt(parameters, "totalCards", 5);
        Guid? categoryId = GetNullableGuid(parameters, "categoryId");
        GameLevel gameLevel = GetEnum(parameters, "gameLevel", GameLevel.Easy);
        int secondsPerCard = GetInt(parameters, "secondsPerCard", 10);
        int optionCount = GetInt(parameters, "optionCount", 4);

        // Extract teams if provided, otherwise create empty list
        var teams = GetTeams(parameters, "teams");

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

    private static List<QuizMultiplayerGameState.TeamData> GetTeams(Dictionary<string, object> parameters, string key)
    {
        if (!parameters.TryGetValue(key, out var value) || value == null)
            return new List<QuizMultiplayerGameState.TeamData>();

        if (value is List<QuizMultiplayerGameState.TeamData> directTeams)
            return directTeams;

        if (value is JsonElement { ValueKind: JsonValueKind.Array } element)
        {
            var payload = element.Deserialize<List<TeamPayload>>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            return payload.Select(ToTeamData).ToList();
        }

        if (value is string json && !string.IsNullOrWhiteSpace(json))
        {
            var payload = JsonSerializer.Deserialize<List<TeamPayload>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            return payload.Select(ToTeamData).ToList();
        }

        return new List<QuizMultiplayerGameState.TeamData>();
    }

    private static QuizMultiplayerGameState.TeamData ToTeamData(TeamPayload payload)
    {
        return new QuizMultiplayerGameState.TeamData
        {
            TeamIndex = payload.TeamIndex,
            Name = payload.Name ?? $"Team {payload.TeamIndex + 1}",
            PlayerIds = payload.PlayerIds ?? new List<string>()
        };
    }

    private sealed class TeamPayload
    {
        public int TeamIndex { get; set; }
        public string? Name { get; set; }
        public List<string>? PlayerIds { get; set; }
    }
}
