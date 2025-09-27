using ComedyPull.Application.Modules.DataSync.Configuration;
using ComedyPull.Application.Modules.DataSync.Services;
using ComedyPull.Application.Modules.DataSync.Services.Interfaces;
using ComedyPull.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
        
        services.AddKeyedSingleton<IScraper, PlaywrightScraper>(DataSourceKeys.Punchup,
            (provider, _) =>
            {
                var options = provider.GetRequiredService<IOptions<DataSyncOptions>>();
                return new PlaywrightScraper(
                    concurrency: options.Value.PunchupCollection.Concurrency
                );
            });
    }
}