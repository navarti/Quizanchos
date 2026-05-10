using Microsoft.Extensions.DependencyInjection;
using Quizanchos.Plugin.TicketToRideEurope.Services;

namespace Quizanchos.Plugin.TicketToRideEurope.Extensions;

public static class TicketToRideEuropeServiceExtensions
{
    public static IServiceCollection AddTicketToRideEuropeServices(this IServiceCollection services)
    {
        services.AddScoped<TicketToRideEuropeEngineFactory>();
        return services;
    }
}
