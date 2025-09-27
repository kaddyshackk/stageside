using ComedyPull.Application.Modules.DataProcessing.Processors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ComedyPull.Application.Modules.DataProcessing;

public static class DataProcessingModuleExtensions
{
    /// <summary>
    /// Configures services for the DataProcessing module.
    /// </summary>
    /// <param name="services">Injected <see cref="IServiceCollection"/> instance.</param>
    public static void AddDataProcessingModule(this IServiceCollection services)
    {
        services.AddScoped<TransformProcessor>();

        services.AddScoped<INotificationHandler<StateCompletedEvent>, StateCompletedHandler>();
    }
}