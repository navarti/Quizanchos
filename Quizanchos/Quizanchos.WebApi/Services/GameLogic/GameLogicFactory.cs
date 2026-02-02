using Quizanchos.Common.Enums;
using Quizanchos.Core;
using Quizanchos.Quiz.GameLogic;
using Quizanchos.Quiz.Services;
using System.Collections.Immutable;
using System.Text.Json;

namespace Quizanchos.WebApi.Services.GameLogic;

public class GameLogicFactory : IGameLogicFactory
{
    private readonly ILogger<GameLogicFactory> _logger;
    private readonly QuizEngineFactory _quizEngineFactory;

    public GameLogicFactory(
        ILogger<GameLogicFactory> logger,
        QuizEngineFactory quizEngineFactory)
    {
        _logger = logger;
        _quizEngineFactory = quizEngineFactory;
    }

    public async Task<IGameEngine> CreateGameEngine(MinigameType type, Guid gameId, ImmutableArray<Guid> playerIds, Dictionary<string, object> parameters)
    {
        _logger.LogInformation("Creating game engine for type: {Type}, GameId: {GameId}, Players: {PlayerCount}", 
            type, gameId, playerIds.Length);
        
        return type switch
        {
            MinigameType.Quiz => await CreateQuizEngine(gameId, playerIds, parameters),
            _ => throw new ArgumentException($"Unknown minigame type: {type}")
        };
    }

    public async Task<IGameEngine?> LoadGameEngine(MinigameType type, Guid gameId)
    {
        _logger.LogInformation("Loading game engine for type: {Type}, GameId: {GameId}", type, gameId);

        return type switch
        {
            MinigameType.Quiz => await LoadQuizEngine(gameId),
            _ => throw new ArgumentException($"Unknown minigame type: {type}")
        };
    }

    public async Task SaveGameState(MinigameType type, Guid gameId, IGameState state)
    {
        _logger.LogInformation("Saving game state for type: {Type}, GameId: {GameId}", type, gameId);

        switch (type)
        {
            case MinigameType.Quiz:
                if (state is QuizGameState quizState)
                {
                    await _quizEngineFactory.SaveQuizStateAsync(gameId, quizState);
                }
                break;
            default:
                throw new ArgumentException($"Unknown minigame type: {type}");
        }
    }

    private async Task<IGameEngine> CreateQuizEngine(Guid gameId, ImmutableArray<Guid> playerIds, Dictionary<string, object> parameters)
    {
        int totalCards = GetParameter<int>(parameters, "totalCards", 10);
        Guid? categoryId = GetParameter<Guid?>(parameters, "categoryId", null);
        GameLevel gameLevel = GetParameter<GameLevel>(parameters, "gameLevel", GameLevel.Easy);
        int secondsPerCard = GetParameter<int>(parameters, "secondsPerCard", 30);
        int optionCount = GetParameter<int>(parameters, "optionCount", 4);

        _logger.LogInformation("Creating Quiz engine with: TotalCards={TotalCards}, CategoryId={CategoryId}, GameLevel={GameLevel}, SecondsPerCard={SecondsPerCard}, OptionCount={OptionCount}",
            totalCards, categoryId, gameLevel, secondsPerCard, optionCount);

        GameEngine<QuizGameState, QuizMove> engine = await _quizEngineFactory.CreateQuizEngineAsync(
            gameId,
            playerIds,
            totalCards,
            categoryId,
            gameLevel,
            secondsPerCard,
            optionCount
        );

        GameEngineWrapper<QuizGameState, QuizMove> wrapper = new GameEngineWrapper<QuizGameState, QuizMove>(engine);
        
        return wrapper;
    }

    private async Task<IGameEngine?> LoadQuizEngine(Guid gameId)
    {
        GameEngine<QuizGameState, QuizMove>? engine = await _quizEngineFactory.LoadQuizEngineAsync(gameId);
        if (engine == null)
            return null;

        return new GameEngineWrapper<QuizGameState, QuizMove>(engine);
    }

    private T GetParameter<T>(Dictionary<string, object> parameters, string key, T defaultValue)
    {
        if (parameters.TryGetValue(key, out object? value))
        {
            // Handle JsonElement from System.Text.Json deserialization
            if (value is JsonElement jsonElement)
            {
                return ConvertJsonElement<T>(jsonElement, defaultValue);
            }
            
            if (value is T typedValue)
            {
                return typedValue;
            }
            
            // Special handling for Guid and Nullable<Guid>
            Type targetType = typeof(T);
            Type? underlyingType = Nullable.GetUnderlyingType(targetType);
            bool isNullable = underlyingType != null;
            Type actualType = underlyingType ?? targetType;

            // Handle Guid conversion from string
            if (actualType == typeof(Guid))
            {
                if (value is string stringValue && Guid.TryParse(stringValue, out Guid guidValue))
                {
                    // For nullable Guid, we need to box it properly
                    if (isNullable)
                    {
                        return (T)(object)(Guid?)guidValue;
                    }
                    return (T)(object)guidValue;
                }
                if (isNullable)
                {
                    return default!;
                }
            }
            
            // Try conversion for numeric types and enums
            try
            {
                if (typeof(T).IsEnum && value is int intValue)
                {
                    return (T)(object)intValue;
                }
                
                if (typeof(T).IsEnum && value is string enumString)
                {
                    return (T)Enum.Parse(typeof(T), enumString, true);
                }
                
                return (T)Convert.ChangeType(value, actualType);
            }
            catch
            {
                return defaultValue;
            }
        }
        return defaultValue;
    }

    private T ConvertJsonElement<T>(JsonElement jsonElement, T defaultValue)
    {
        Type targetType = typeof(T);
        Type? underlyingType = Nullable.GetUnderlyingType(targetType);
        bool isNullable = underlyingType != null;
        Type actualType = underlyingType ?? targetType;

        try
        {
            // Handle null values
            if (jsonElement.ValueKind == JsonValueKind.Null)
            {
                return isNullable ? default! : defaultValue;
            }

            // Handle Guid
            if (actualType == typeof(Guid))
            {
                if (jsonElement.ValueKind == JsonValueKind.String)
                {
                    string? stringValue = jsonElement.GetString();
                    if (!string.IsNullOrEmpty(stringValue) && Guid.TryParse(stringValue, out Guid guidValue))
                    {
                        if (isNullable)
                        {
                            return (T)(object)(Guid?)guidValue;
                        }
                        return (T)(object)guidValue;
                    }
                }
                return isNullable ? default! : defaultValue;
            }

            // Handle enums
            if (actualType.IsEnum)
            {
                if (jsonElement.ValueKind == JsonValueKind.Number)
                {
                    int enumValue = jsonElement.GetInt32();
                    return (T)Enum.ToObject(actualType, enumValue);
                }
                if (jsonElement.ValueKind == JsonValueKind.String)
                {
                    string? enumString = jsonElement.GetString();
                    if (!string.IsNullOrEmpty(enumString))
                    {
                        return (T)Enum.Parse(actualType, enumString, true);
                    }
                }
                return defaultValue;
            }

            // Handle primitives
            if (actualType == typeof(int))
                return (T)(object)jsonElement.GetInt32();
            if (actualType == typeof(long))
                return (T)(object)jsonElement.GetInt64();
            if (actualType == typeof(double))
                return (T)(object)jsonElement.GetDouble();
            if (actualType == typeof(float))
                return (T)(object)jsonElement.GetSingle();
            if (actualType == typeof(bool))
                return (T)(object)jsonElement.GetBoolean();
            if (actualType == typeof(string))
                return (T)(object)jsonElement.GetString()!;

            // Try JsonSerializer.Deserialize as fallback
            return jsonElement.Deserialize<T>() ?? defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }
}
