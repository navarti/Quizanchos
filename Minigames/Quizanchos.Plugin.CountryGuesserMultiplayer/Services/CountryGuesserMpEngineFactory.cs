using System.Collections.Immutable;
using System.Text.Json;
using Quizanchos.Core;
using Quizanchos.Plugin.CountryGuesser.Data;
using Quizanchos.Plugin.CountryGuesserMultiplayer.GameLogic;

namespace Quizanchos.Plugin.CountryGuesserMultiplayer.Services;

public sealed class CountryGuesserMpEngineFactory
{
    private readonly IGameStatePersistence _persistence;
    private readonly CountryRepository _repository;

    public CountryGuesserMpEngineFactory(IGameStatePersistence persistence, CountryRepository repository)
    {
        _persistence = persistence;
        _repository = repository;
    }

    public async Task<GameEngine<CountryGuesserMpState, CountryGuesserMpMove>> CreateAsync(
        Guid gameId,
        ImmutableArray<string> playerIds,
        int totalCards,
        int optionCount,
        int secondsPerCard,
        int seed)
    {
        var logic = new CountryGuesserMpLogic(_repository, totalCards, optionCount, secondsPerCard, seed);
        var engine = new GameEngine<CountryGuesserMpState, CountryGuesserMpMove>(logic, gameId, playerIds);

        await _persistence.CreateAsync(
            gameId,
            CountryGuesserMpConstants.MinigameTypeId,
            playerIds.ToArray(),
            JsonSerializer.Serialize(engine.State));

        return engine;
    }

    public async Task<GameEngine<CountryGuesserMpState, CountryGuesserMpMove>?> LoadAsync(Guid gameId)
    {
        var loaded = await _persistence.LoadAsync(gameId);
        if (loaded is null) return null;

        var state = JsonSerializer.Deserialize<CountryGuesserMpState>(loaded.StateJson);
        if (state is null) return null;

        state.GameId = gameId;
        state.Players = loaded.PlayerIds;
        state.IsFinished = loaded.IsFinished;
        state.Winner = loaded.Winner;

        var logic = new CountryGuesserMpLogic(_repository, state.TotalCards, state.OptionCount, state.SecondsPerCard);
        return new GameEngine<CountryGuesserMpState, CountryGuesserMpMove>(logic, state);
    }

    public async Task SaveAsync(Guid gameId, CountryGuesserMpState state)
    {
        await _persistence.UpdateAsync(gameId, JsonSerializer.Serialize(state));
    }
}
