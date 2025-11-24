using MassTransit;
using StageSide.Scheduler.Service.Dispatching;

namespace StageSide.Scheduler.Service.Extensions;

public static class ServiceExtensions
{
    public static void AddServiceLayer(this IServiceCollection services, IConfiguration configuration)
    {
        // Dispatching services
        services.Configure<DispatchingServiceOptions>(configuration.GetSection("DispatchingService"));
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