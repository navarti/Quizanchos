using System.Collections.Immutable;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Quizanchos.Core;
using Quizanchos.Plugin.Caravan.Extensions;
using Quizanchos.Plugin.Caravan.GameLogic;
using Quizanchos.Plugin.Caravan.Services;

namespace Quizanchos.Plugin.Caravan.Descriptors;

public sealed class CaravanMinigameDescriptor : IMinigameDescriptor
{
    public int MinigameTypeId => CaravanConstants.MinigameTypeId;
    public string GameKey => CaravanConstants.GameKey;
    public string DisplayName => CaravanConstants.DisplayName;
    public bool IsPremium => false;
    public Type MoveType => typeof(CaravanMove);
    public string MoveDiscriminator => "caravan";

    public void RegisterServices(IServiceCollection services)
    {
        services.AddCaravanServices();
    }

    public async Task<IGameEngine> CreateGameEngineAsync(
        Guid gameId,
        ImmutableArray<string> playerIds,
        Dictionary<string, object> parameters,
        IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetRequiredService<CaravanEngineFactory>();
        int seed = GetInt(parameters, "seed", 0);
        var engine = await factory.CreateAsync(gameId, playerIds, seed);
        return new GameEngineWrapper<CaravanState, CaravanMove>(engine);
    }

    public async Task<IGameEngine?> LoadGameEngineAsync(Guid gameId, IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetRequiredService<CaravanEngineFactory>();
        var engine = await factory.LoadAsync(gameId);
        return engine is null ? null : new GameEngineWrapper<CaravanState, CaravanMove>(engine);
    }

    public async Task SaveGameStateAsync(Guid gameId, IGameState state, IServiceProvider serviceProvider)
    {
        if (state is not CaravanState typed)
        {
            return;
        }
        var factory = serviceProvider.GetRequiredService<CaravanEngineFactory>();
        await factory.SaveAsync(gameId, typed);
    }

    private static int GetInt(Dictionary<string, object> parameters, string key, int fallback)
    {
        if (!parameters.TryGetValue(key, out var raw) || raw is null)
        {
            return fallback;
        }
        return raw switch
        {
            int i => i,
            long l => (int)l,
            string s when int.TryParse(s, out var parsed) => parsed,
            JsonElement { ValueKind: JsonValueKind.Number } n when n.TryGetInt32(out var parsed) => parsed,
            _ => fallback,
        };
    }
}
