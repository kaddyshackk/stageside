using ComedyPull.Application.Modules.DataProcessing.Events;
using ComedyPull.Application.Modules.DataProcessing.Processors;
using ComedyPull.Application.Modules.DataProcessing.Processors.Interfaces;
using ComedyPull.Application.Modules.DataProcessing.Processors.SubProcessors;
using ComedyPull.Application.Modules.DataProcessing.Services;
using ComedyPull.Application.Modules.DataProcessing.Services.Interfaces;
using ComedyPull.Domain.Enums;
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
        // Register sub-processor resolver
        services.AddScoped<ISubProcessorResolver, SubProcessorResolver>();

        // Register state processors
        services.AddScoped<IStateProcessor, TransformStateProcessor>();

        // Register generic sub-processors (fallbacks)
        services.AddScoped<ISubProcessor<DataSource>, GenericTransformSubProcessor>();

        // Register event handlers
        services.AddScoped<INotificationHandler<StateCompletedEvent>, StateCompletedHandler>();
    }
}