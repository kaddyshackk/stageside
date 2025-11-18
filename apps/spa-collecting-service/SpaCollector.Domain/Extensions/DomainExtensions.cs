using Microsoft.Extensions.DependencyInjection;
using StageSide.Pipeline.Interfaces;
using StageSide.SpaCollector.Domain.Collection;

namespace StageSide.SpaCollector.Domain.Extensions;

public static class DomainExtensions
{
    public static void AddDomainLayer(this IServiceCollection services)
    {
        services.AddScoped<IScheduler, GenericSitemapScheduler>();
        services.AddSingleton<IPipelineAdapterFactory, PipelineAdapterFactory>();
    }
}