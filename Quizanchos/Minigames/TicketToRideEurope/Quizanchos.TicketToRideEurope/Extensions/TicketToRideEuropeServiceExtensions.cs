using Microsoft.Extensions.DependencyInjection;

namespace Quizanchos.TicketToRideEurope.Extensions;

public static class TicketToRideEuropeServiceExtensions
{
    public static IServiceCollection AddTicketToRideEuropeServices(this IServiceCollection services)
    {
        services.AddScoped<Services.TicketToRideEuropeStateService>();
        services.AddScoped<Services.TicketToRideEuropeEngineFactory>();
        return services;
    }
}
