using Quizanchos.Common.Enums;
using Quizanchos.Core;
using Quizanchos.Quiz.GameLogic;
using Quizanchos.Quiz.Services;
using System.Collections.Immutable;
using System.Text.Json;

namespace Quizanchos.WebApi.Services;

public class GameLogicFactory : IGameLogicFactory
{
    private readonly ILogger<GameLogicFactory> _logger;
    private readonly IServiceProvider _serviceProvider;

    public GameLogicFactory(
        ILogger<GameLogicFactory> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public IGameEngine CreateGameEngine(MinigameType type, Guid gameId, ImmutableArray<Guid> playerIds, Dictionary<string, object> parameters)
    {
        _logger.LogInformation("Creating game engine for type: {Type}, GameId: {GameId}, Players: {PlayerCount}", 
            type, gameId, playerIds.Length);

        return type switch
        {
            MinigameType.Quiz => CreateQuizEngine(gameId, playerIds, parameters),
            _ => throw new ArgumentException($"Unknown minigame type: {type}")
        };
    }

    private IGameEngine CreateQuizEngine(Guid gameId, ImmutableArray<Guid> playerIds, Dictionary<string, object> parameters)
    {
        int totalCards = GetParameter<int>(parameters, "totalCards", 10);
        Guid? categoryId = GetParameter<Guid?>(parameters, "categoryId", null);
        GameLevel gameLevel = GetParameter<GameLevel>(parameters, "gameLevel", GameLevel.Easy);
        int secondsPerCard = GetParameter<int>(parameters, "secondsPerCard", 30);
        int optionCount = GetParameter<int>(parameters, "optionCount", 4);

        _logger.LogInformation("Creating Quiz engine with: TotalCards={TotalCards}, CategoryId={CategoryId}, GameLevel={GameLevel}, SecondsPerCard={SecondsPerCard}, OptionCount={OptionCount}",
            totalCards, categoryId, gameLevel, secondsPerCard, optionCount);

        // Get the card generator service
        QuizCardGeneratorService? cardGenerator = null;
        using (var scope = _serviceProvider.CreateScope())
        {
            cardGenerator = scope.ServiceProvider.GetService<QuizCardGeneratorService>();
        }

        QuizGameLogic logic = new QuizGameLogic(
            totalCards,
            categoryId,
            gameLevel,
            secondsPerCard,
            optionCount,
            cardGenerator
        );

        GameEngine<QuizGameState, QuizMove> engine = new GameEngine<QuizGameState, QuizMove>(logic, gameId, playerIds);
        GameEngineWrapper<QuizGameState, QuizMove> wrapper = new GameEngineWrapper<QuizGameState, QuizMove>(engine);
        
        QuizGameState state = engine.State;
        _logger.LogInformation("Quiz engine created. Initial state: CurrentCardIndex={CurrentCardIndex}, TotalCards={TotalCards}, Cards.Count={CardsCount}",
            state.CurrentCardIndex, state.TotalCards, state.Cards.Count);

        // Generate cards if categoryId is provided
        if (categoryId.HasValue && categoryId.Value != Guid.Empty && cardGenerator != null)
        {
            _logger.LogInformation("Delegating card generation to QuizCardGeneratorService");
            
            Task.Run(async () =>
            {
                try
                {
                    await cardGenerator.GenerateCardsForGame(state, categoryId.Value, totalCards, optionCount, gameLevel);
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
        
        return wrapper;
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
