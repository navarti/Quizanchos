using Microsoft.Extensions.DependencyInjection;
using Quizanchos.TicketToRideEurope.Services;

namespace Quizanchos.TicketToRideEurope.Extensions;

public static class TicketToRideEuropeServiceExtensions
{
    public static IServiceCollection AddTicketToRideEuropeServices(this IServiceCollection services)
    {
        services.AddScoped<TicketToRideEuropeStateService>();
        services.AddScoped<TicketToRideEuropeEngineFactory>();
        return services;
    }
}
