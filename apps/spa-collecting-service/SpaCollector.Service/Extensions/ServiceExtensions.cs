using Coravel;
using MassTransit;
using StageSide.SpaCollector.Service.Collection;

namespace StageSide.SpaCollector.Service.Extensions;

public static class ServiceExtensions
{
    public static void AddServiceLayer(this IServiceCollection services, IConfiguration configuration)
    {
        // MassTransit
        services.AddMassTransit(x =>
        {
            x.AddConsumer<SpaCollectionConsumer>();
            x.SetKebabCaseEndpointNameFormatter();
            x.UsingInMemory((context, config) =>
            {
                config.ConfigureEndpoints(context);
            });
        });
        
        // Coravel
        services.AddQueue();
    }
}