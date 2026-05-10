using System.Collections.Immutable;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Quizanchos.Core;
using Quizanchos.Plugin.CountryGuesserMultiplayer.Extensions;
using Quizanchos.Plugin.CountryGuesserMultiplayer.GameLogic;
using Quizanchos.Plugin.CountryGuesserMultiplayer.Services;

namespace Quizanchos.Plugin.CountryGuesserMultiplayer.Descriptors;

public sealed class CountryGuesserMpMinigameDescriptor : IMinigameDescriptor
{
    public int MinigameTypeId => CountryGuesserMpConstants.MinigameTypeId;
    public string GameKey => CountryGuesserMpConstants.GameKey;
    public string DisplayName => CountryGuesserMpConstants.DisplayName;
    public bool IsPremium => false;
    public Type MoveType => typeof(CountryGuesserMpMove);
    public string MoveDiscriminator => "countryGuesserMp";

    public void RegisterServices(IServiceCollection services)
    {
        services.AddCountryGuesserMpServices();
    }

    public async Task<IGameEngine> CreateGameEngineAsync(
        Guid gameId,
        ImmutableArray<string> playerIds,
        Dictionary<string, object> parameters,
        IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetRequiredService<CountryGuesserMpEngineFactory>();
        int totalCards = GetInt(parameters, "totalCards", 5);
        int secondsPerCard = GetInt(parameters, "secondsPerCard", 20);
        double maxDistanceKm = GetDouble(parameters, "maxDistanceKm", 600);
        int seed = GetInt(parameters, "seed", 0);
        var engine = await factory.CreateAsync(gameId, playerIds, totalCards, secondsPerCard, maxDistanceKm, seed);
        return new GameEngineWrapper<CountryGuesserMpState, CountryGuesserMpMove>(engine);
    }

    public async Task<IGameEngine?> LoadGameEngineAsync(Guid gameId, IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetRequiredService<CountryGuesserMpEngineFactory>();
        var engine = await factory.LoadAsync(gameId);
        return engine is null ? null : new GameEngineWrapper<CountryGuesserMpState, CountryGuesserMpMove>(engine);
    }

    public async Task SaveGameStateAsync(Guid gameId, IGameState state, IServiceProvider serviceProvider)
    {
        if (state is not CountryGuesserMpState typed) return;
        var factory = serviceProvider.GetRequiredService<CountryGuesserMpEngineFactory>();
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

    private static double GetDouble(Dictionary<string, object> parameters, string key, double fallback)
    {
        if (!parameters.TryGetValue(key, out var raw) || raw is null) return fallback;
        return raw switch
        {
            double d => d,
            float f => f,
            int i => i,
            long l => l,
            string s when double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var p) => p,
            JsonElement { ValueKind: JsonValueKind.Number } n when n.TryGetDouble(out var p) => p,
            JsonElement { ValueKind: JsonValueKind.String } e when double.TryParse(e.GetString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var p) => p,
            _ => fallback,
        };
    }
}
