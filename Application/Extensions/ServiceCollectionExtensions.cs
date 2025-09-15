using ComedyPull.Application.Features.DataProcessing.Services;
using ComedyPull.Application.Features.DataSync.Interfaces;
using ComedyPull.Application.Features.DataSync.Punchup;
using ComedyPull.Application.Features.DataSync.Services;
using ComedyPull.Application.Options;
using ComedyPull.Domain.Enums;
using ComedyPull.Domain.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ComedyPull.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configures services for the Application layer.
        /// </summary>
        /// <param name="services">Injected <see cref="IServiceCollection"/> instance.</param>
        /// <param name="configuration">Injected <see cref="IConfiguration"/> instance.</param>
        public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDataSyncServices(configuration);
            services.AddDataProcessingServices(configuration);
        }

        /// <summary>
        /// Configures services for the DataSync feature.
        /// </summary>
        /// <param name="services">Injected <see cref="IServiceCollection"/> instance.</param>
        /// <param name="configuration">Injected <see cref="IConfiguration"/> instance.</param>
        private static void AddDataSyncServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ScrapeOptions>(configuration.GetSection("ScrapeSettings"));
            services.AddScoped<ISitemapLoader, SitemapLoader>();

            // Processors
            services.AddTransient<PunchupTicketsPageProcessor>();

            // Jobs
            services.AddScoped<PunchupScrapeJob>();

            // Scrapers
            services.AddKeyedSingleton<IScraper, PlaywrightScraper>(DataSourceKeys.Punchup,
                (provider, _) =>
                {
                    var options = provider.GetRequiredService<IOptions<ScrapeOptions>>();
                    return new PlaywrightScraper(
                        concurrency: options.Value.Punchup.Concurrency
                    );
                });
        }

        /// <summary>
        /// Configures services for the DataProcessing feature.
        /// </summary>
        /// <param name="services">Injected <see cref="IServiceCollection"/> instance.</param>
        /// <param name="configuration">Injected <see cref="IConfiguration"/> instance.</param>
        private static void AddDataProcessingServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Options
            services.Configure<DataProcessingOptions>(configuration.GetSection("DataProcessingOptions"));

            // Services
            services.AddHostedService<BronzeProcessingService>();
        }
    }
}