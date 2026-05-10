using System.Collections.Immutable;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Quizanchos.Core;
using Quizanchos.Plugin.CaravanMultiplayer.Extensions;
using Quizanchos.Plugin.CaravanMultiplayer.GameLogic;
using Quizanchos.Plugin.CaravanMultiplayer.Services;

namespace Quizanchos.Plugin.CaravanMultiplayer.Descriptors;

public sealed class CaravanMpMinigameDescriptor : IMinigameDescriptor
{
    public int MinigameTypeId => CaravanMpConstants.MinigameTypeId;
    public string GameKey => CaravanMpConstants.GameKey;
    public string DisplayName => CaravanMpConstants.DisplayName;
    public bool IsPremium => false;
    public Type MoveType => typeof(CaravanMpMove);
    public string MoveDiscriminator => "caravanMp";

    public void RegisterServices(IServiceCollection services)
    {
        services.AddCaravanMpServices();
    }

    public async Task<IGameEngine> CreateGameEngineAsync(
        Guid gameId,
        ImmutableArray<string> playerIds,
        Dictionary<string, object> parameters,
        IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetRequiredService<CaravanMpEngineFactory>();
        int seed = GetInt(parameters, "seed", 0);
        var nicknames = GetNicknames(parameters);
        var engine = await factory.CreateAsync(gameId, playerIds, seed, nicknames);
        return new GameEngineWrapper<CaravanMpState, CaravanMpMove>(engine);
    }

    public async Task<IGameEngine?> LoadGameEngineAsync(Guid gameId, IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetRequiredService<CaravanMpEngineFactory>();
        var engine = await factory.LoadAsync(gameId);
        return engine is null ? null : new GameEngineWrapper<CaravanMpState, CaravanMpMove>(engine);
    }

    public async Task SaveGameStateAsync(Guid gameId, IGameState state, IServiceProvider serviceProvider)
    {
        if (state is not CaravanMpState typed) return;
        var factory = serviceProvider.GetRequiredService<CaravanMpEngineFactory>();
        await factory.SaveAsync(gameId, typed);
    }

    private static int GetInt(Dictionary<string, object> parameters, string key, int fallback)
    {
        if (!parameters.TryGetValue(key, out var raw) || raw is null) return fallback;
        return raw switch
        {
            int i => i,
            long l => (int)l,
            string s when int.TryParse(s, out var p) => p,
            JsonElement { ValueKind: JsonValueKind.Number } n when n.TryGetInt32(out var p) => p,
            JsonElement { ValueKind: JsonValueKind.String } e when int.TryParse(e.GetString(), out var p) => p,
            _ => fallback,
        };
    }

    private static Dictionary<string, string> GetNicknames(Dictionary<string, object> parameters)
    {
        if (!parameters.TryGetValue("playerNicknames", out var raw) || raw is null)
        {
            return new Dictionary<string, string>();
        }

        try
        {
            string? json = raw switch
            {
                string s => s,
                JsonElement { ValueKind: JsonValueKind.String } e => e.GetString(),
                JsonElement el => el.GetRawText(),
                _ => null,
            };
            if (string.IsNullOrWhiteSpace(json)) return new Dictionary<string, string>();
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                ?? new Dictionary<string, string>();
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }
}
