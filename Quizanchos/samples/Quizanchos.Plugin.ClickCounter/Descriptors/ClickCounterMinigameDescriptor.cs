using System.Collections.Immutable;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Quizanchos.Core;
using Quizanchos.Plugin.ClickCounter.Extensions;
using Quizanchos.Plugin.ClickCounter.GameLogic;
using Quizanchos.Plugin.ClickCounter.Services;

namespace Quizanchos.Plugin.ClickCounter.Descriptors;

public sealed class ClickCounterMinigameDescriptor : IMinigameDescriptor
{
    public int MinigameTypeId => ClickCounterConstants.MinigameTypeId;
    public string GameKey => ClickCounterConstants.GameKey;
    public string DisplayName => "Click Counter";
    public bool IsPremium => false;
    public Type MoveType => typeof(ClickCounterMove);
    public string MoveDiscriminator => "clickcounter";

    public void RegisterServices(IServiceCollection services)
    {
        services.AddClickCounterServices();
    }

    public async Task<IGameEngine> CreateGameEngineAsync(
        Guid gameId,
        ImmutableArray<string> playerIds,
        Dictionary<string, object> parameters,
        IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetRequiredService<ClickCounterEngineFactory>();
        var target = GetInt(parameters, "target", 10);
        var engine = await factory.CreateAsync(gameId, playerIds, target);
        return new GameEngineWrapper<ClickCounterState, ClickCounterMove>(engine);
    }

    public async Task<IGameEngine?> LoadGameEngineAsync(Guid gameId, IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetRequiredService<ClickCounterEngineFactory>();
        var engine = await factory.LoadAsync(gameId);
        return engine is null ? null : new GameEngineWrapper<ClickCounterState, ClickCounterMove>(engine);
    }

    public async Task SaveGameStateAsync(Guid gameId, IGameState state, IServiceProvider serviceProvider)
    {
        if (state is not ClickCounterState typed)
        {
            return;
        }
        var factory = serviceProvider.GetRequiredService<ClickCounterEngineFactory>();
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
