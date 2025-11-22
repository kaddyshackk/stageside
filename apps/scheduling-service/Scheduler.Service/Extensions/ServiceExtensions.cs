using MassTransit;
using StageSide.Scheduler.Service.Dispatching;

namespace StageSide.Scheduler.Service.Extensions;

public static class ServiceExtensions
{
    public static void AddServiceLayer(this IServiceCollection services)
    {
        // Dispatching services
        services.AddHostedService<DispatchingService>();
        
        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.UsingInMemory((context, config) =>
            {
                config.ConfigureEndpoints(context);
            });
        });
    }
}