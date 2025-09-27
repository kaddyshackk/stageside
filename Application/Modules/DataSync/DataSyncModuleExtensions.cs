using ComedyPull.Application.Modules.DataSync.Configuration;
using ComedyPull.Application.Modules.DataSync.Services;
using ComedyPull.Application.Modules.DataSync.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ComedyPull.Application.Modules.DataSync;

public static class DataSyncModuleExtensions
{
    /// <summary>
    /// Configures services for the DataSync module.
    /// </summary>
    /// <param name="services">Injected <see cref="IServiceCollection"/> instance.</param>
    /// <param name="configuration">Injected <see cref="IConfiguration"/> instance.</param>
    public static void AddDataSyncModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DataSyncOptions>(configuration.GetSection("DataSyncOptions"));

        services.AddScoped<ISitemapLoader, SitemapLoader>();

        services.AddHostedService<SourceRecordIngestionService>();

        services.AddScoped<IPlaywrightScraperFactory, PlaywrightScraperFactory>();
    }
}