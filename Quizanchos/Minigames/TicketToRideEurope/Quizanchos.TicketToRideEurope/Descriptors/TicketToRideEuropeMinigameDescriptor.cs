using Microsoft.Extensions.DependencyInjection;
using Quizanchos.Core;
using Quizanchos.TicketToRideEurope.Extensions;
using Quizanchos.TicketToRideEurope.GameLogic;
using Quizanchos.TicketToRideEurope.Services;
using System.Collections.Immutable;

namespace Quizanchos.TicketToRideEurope.Descriptors;

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

        GameEngine<TicketToRideEuropeState, TicketToRideEuropeMove> engine =
            await factory.CreateEngineAsync(gameId, playerIds);

        return new GameEngineWrapper<TicketToRideEuropeState, TicketToRideEuropeMove>(engine);
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
