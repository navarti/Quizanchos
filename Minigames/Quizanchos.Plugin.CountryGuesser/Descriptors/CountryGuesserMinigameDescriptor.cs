using System.Collections.Immutable;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Quizanchos.Core;
using Quizanchos.Plugin.CountryGuesser.Extensions;
using Quizanchos.Plugin.CountryGuesser.GameLogic;
using Quizanchos.Plugin.CountryGuesser.Services;

namespace Quizanchos.Plugin.CountryGuesser.Descriptors;

public sealed class CountryGuesserMinigameDescriptor : IMinigameDescriptor
{
    public int MinigameTypeId => CountryGuesserConstants.MinigameTypeId;
    public string GameKey => CountryGuesserConstants.GameKey;
    public string DisplayName => CountryGuesserConstants.DisplayName;
    public bool IsPremium => false;
    public Type MoveType => typeof(CountryGuesserMove);
    public string MoveDiscriminator => "countryGuesser";

    public void RegisterServices(IServiceCollection services)
    {
        services.AddCountryGuesserServices();
    }

    public async Task<IGameEngine> CreateGameEngineAsync(
        Guid gameId,
        ImmutableArray<string> playerIds,
        Dictionary<string, object> parameters,
        IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetRequiredService<CountryGuesserEngineFactory>();
        int totalCards = GetInt(parameters, "totalCards", 5);
        int optionCount = GetInt(parameters, "optionCount", 4);
        int secondsPerCard = GetInt(parameters, "secondsPerCard", 20);
        int seed = GetInt(parameters, "seed", 0);

        var engine = await factory.CreateAsync(gameId, playerIds, totalCards, optionCount, secondsPerCard, seed);
        return new GameEngineWrapper<CountryGuesserState, CountryGuesserMove>(engine);
    }

    public async Task<IGameEngine?> LoadGameEngineAsync(Guid gameId, IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetRequiredService<CountryGuesserEngineFactory>();
        var engine = await factory.LoadAsync(gameId);
        return engine is null ? null : new GameEngineWrapper<CountryGuesserState, CountryGuesserMove>(engine);
    }

    public async Task SaveGameStateAsync(Guid gameId, IGameState state, IServiceProvider serviceProvider)
    {
        if (state is not CountryGuesserState typed) return;
        var factory = serviceProvider.GetRequiredService<CountryGuesserEngineFactory>();
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
}
