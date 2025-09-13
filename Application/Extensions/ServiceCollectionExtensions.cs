using ComedyPull.Application.Features.DataProcessing.Services;
using ComedyPull.Application.Features.Ingest.Interfaces;
using ComedyPull.Application.Features.Ingest.Punchup;
using ComedyPull.Application.Features.Ingest.Services;
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
            services.AddIngestServices(configuration);
            services.AddDataProcessingServices(configuration);
        }
        
        /// <summary>
        /// Configures services for the Ingest feature.
        /// </summary>
        /// <param name="services">Injected <see cref="IServiceCollection"/> instance.</param>
        /// <param name="configuration">Injected <see cref="IConfiguration"/> instance.</param>
        private static void AddIngestServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ScrapeOptions>(configuration.GetSection("ScrapeSettings"));
            services.AddScoped<ISitemapLoader, SitemapLoader>();
            
            // Processors
            services.AddTransient<PunchupTicketsPageProcessor>();
            
            // Jobs
            services.AddScoped<PunchupScrapeJob>();
            
            // Scrapers
            services.AddKeyedSingleton<IScraper, PlaywrightScraper>(DataSource.Punchup.GetEnumDescription(), (provider, _) =>
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