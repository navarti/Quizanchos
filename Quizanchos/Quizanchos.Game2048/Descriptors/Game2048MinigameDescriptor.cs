using Microsoft.Extensions.DependencyInjection;
using Quizanchos.Core;
using Quizanchos.Game2048.Extensions;
using Quizanchos.Game2048.GameLogic;
using Quizanchos.Game2048.Services;
using System.Collections.Immutable;
using System.Text.Json;

namespace Quizanchos.Game2048.Descriptors;

/// <summary>
/// Descriptor for the 2048 minigame plugin.
/// Handles registration and lifecycle management of 2048 game instances.
/// </summary>
public class Game2048MinigameDescriptor : IMinigameDescriptor
{
    public int MinigameTypeId => 2;
    public string GameKey => "Game2048";
    public string DisplayName => "2048";
    public Type MoveType => typeof(Game2048Move);
    public string MoveDiscriminator => "game2048";

    public void RegisterServices(IServiceCollection services)
    {
        // Register all 2048-specific repositories and services
        services.AddGame2048Services();
    }

    public async Task<IGameEngine> CreateGameEngineAsync(Guid gameId, ImmutableArray<string> playerIds,
        Dictionary<string, object> parameters, IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetRequiredService<Game2048EngineFactory>();

        // Extract game parameters with defaults
        int size = GetInt(parameters, "size", 4);

        // Create the game engine
        var engine = await factory.CreateGame2048EngineAsync(gameId, playerIds, size);

        // Wrap the engine to comply with IGameEngine interface
        return new GameEngineWrapper<Game2048State, Game2048Move>(engine);
    }

    public async Task<IGameEngine?> LoadGameEngineAsync(Guid gameId, IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetRequiredService<Game2048EngineFactory>();

        var engine = await factory.LoadGame2048EngineAsync(gameId);
        if (engine == null)
            return null;

        return new GameEngineWrapper<Game2048State, Game2048Move>(engine);
    }

    public async Task SaveGameStateAsync(Guid gameId, IGameState state, IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetRequiredService<Game2048EngineFactory>();

        if (state is Game2048State game2048State)
        {
            await factory.SaveGame2048StateAsync(gameId, game2048State);
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
}
