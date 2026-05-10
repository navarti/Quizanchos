using Microsoft.Extensions.DependencyInjection;
using Quizanchos.Core;
using Quizanchos.Plugin.TicketToRideEurope.Extensions;
using Quizanchos.Plugin.TicketToRideEurope.GameLogic;
using Quizanchos.Plugin.TicketToRideEurope.Services;
using System.Collections.Immutable;
using System.Text.Json;

namespace Quizanchos.Plugin.TicketToRideEurope.Descriptors;

public class TicketToRideEuropeMinigameDescriptor : IMinigameDescriptor
{
    public int MinigameTypeId => TicketToRideEuropeState.MinigameTypeId;
    public string GameKey => "TicketToRideEurope";
    public string DisplayName => "Ticket to Ride: Europe";
    public bool IsPremium => true;
    public Type MoveType => typeof(TicketToRideEuropeMove);
    public string MoveDiscriminator => "ticketToRideEurope";

    public void RegisterServices(IServiceCollection services)
    {
        services.AddTicketToRideEuropeServices();
    }

    public async Task<IGameEngine> CreateGameEngineAsync(Guid gameId, ImmutableArray<string> playerIds,
        Dictionary<string, object> parameters, IServiceProvider serviceProvider)
    {
        TicketToRideEuropeEngineFactory factory =
            serviceProvider.GetRequiredService<TicketToRideEuropeEngineFactory>();

        Dictionary<string, string> nicknames = ReadNicknames(parameters);

        GameEngine<TicketToRideEuropeState, TicketToRideEuropeMove> engine =
            await factory.CreateEngineAsync(gameId, playerIds, nicknames);

        return new GameEngineWrapper<TicketToRideEuropeState, TicketToRideEuropeMove>(engine);
    }

    private static Dictionary<string, string> ReadNicknames(Dictionary<string, object> parameters)
    {
        if (!parameters.TryGetValue("playerNicknames", out object? value) || value is null)
            return new Dictionary<string, string>();

        return value switch
        {
            Dictionary<string, string> direct => new Dictionary<string, string>(direct),
            IReadOnlyDictionary<string, string> readOnly => new Dictionary<string, string>(readOnly),
            string json when !string.IsNullOrWhiteSpace(json) =>
                JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new(),
            JsonElement { ValueKind: JsonValueKind.Object } element =>
                element.Deserialize<Dictionary<string, string>>() ?? new(),
            _ => new Dictionary<string, string>()
        };
    }

    public async Task<IGameEngine?> LoadGameEngineAsync(Guid gameId, IServiceProvider serviceProvider)
    {
        TicketToRideEuropeEngineFactory factory =
            serviceProvider.GetRequiredService<TicketToRideEuropeEngineFactory>();

        GameEngine<TicketToRideEuropeState, TicketToRideEuropeMove>? engine =
            await factory.LoadEngineAsync(gameId);
        if (engine == null) return null;

        return new GameEngineWrapper<TicketToRideEuropeState, TicketToRideEuropeMove>(engine);
    }

    public async Task SaveGameStateAsync(Guid gameId, IGameState state, IServiceProvider serviceProvider)
    {
        TicketToRideEuropeEngineFactory factory =
            serviceProvider.GetRequiredService<TicketToRideEuropeEngineFactory>();

        if (state is TicketToRideEuropeState typedState)
        {
            await factory.SaveStateAsync(gameId, typedState);
        }
    }
}
