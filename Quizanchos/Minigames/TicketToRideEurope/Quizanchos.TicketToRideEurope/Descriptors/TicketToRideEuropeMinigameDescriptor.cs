using Microsoft.Extensions.DependencyInjection;
using Quizanchos.Core;
using Quizanchos.TicketToRideEurope.Extensions;
using Quizanchos.TicketToRideEurope.GameLogic;
using Quizanchos.TicketToRideEurope.Services;
using System.Collections.Immutable;

namespace Quizanchos.TicketToRideEurope.Descriptors;

public class TicketToRideEuropeMinigameDescriptor : IMinigameDescriptor
{
    public int MinigameTypeId => 4;
    public string GameKey => "TicketToRideEurope";
    public string DisplayName => "Ticket to Ride: Europe";
    public Type MoveType => typeof(TicketToRideEuropeMove);
    public string MoveDiscriminator => "ticketToRideEurope";

    public void RegisterServices(IServiceCollection services)
    {
        services.AddTicketToRideEuropeServices();
    }

    public async Task<IGameEngine> CreateGameEngineAsync(
        Guid gameId,
        ImmutableArray<string> playerIds,
        Dictionary<string, object> parameters,
        IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetRequiredService<TicketToRideEuropeEngineFactory>();
        var engine = await factory.CreateEngineAsync(gameId, playerIds);
        return new GameEngineWrapper<TicketToRideEuropeState, TicketToRideEuropeMove>(engine);
    }

    public async Task<IGameEngine?> LoadGameEngineAsync(Guid gameId, IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetRequiredService<TicketToRideEuropeEngineFactory>();
        var engine = await factory.LoadEngineAsync(gameId);
        return engine == null
            ? null
            : new GameEngineWrapper<TicketToRideEuropeState, TicketToRideEuropeMove>(engine);
    }

    public async Task SaveGameStateAsync(Guid gameId, IGameState state, IServiceProvider serviceProvider)
    {
        if (state is not TicketToRideEuropeState ttrState)
            return;

        var factory = serviceProvider.GetRequiredService<TicketToRideEuropeEngineFactory>();
        await factory.SaveStateAsync(gameId, ttrState);
    }
}
