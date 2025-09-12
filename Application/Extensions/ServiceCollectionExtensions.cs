using ComedyPull.Application.Features.Ingest.Interfaces;
using ComedyPull.Application.Features.Ingest.Punchup;
using ComedyPull.Application.Features.Ingest.Services;
using ComedyPull.Application.Options;
using ComedyPull.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ComedyPull.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDataSyncServices(configuration);
        }
        
        private static void AddDataSyncServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ScrapeOptions>(configuration.GetSection("ScrapeSettings"));
            services.AddScoped<ISitemapLoader, SitemapLoader>();
            
            // Processors
            services.AddTransient<PunchupTicketsPageProcessor>();
            
            // Jobs
            services.AddScoped<PunchupScrapeJob>();
            
            // Scrapers
            services.AddKeyedSingleton<IScraper, PlaywrightScraper>(DataSource.Punchup, (provider, _) =>
            {
                var options = provider.GetRequiredService<IOptions<ScrapeOptions>>();
                return new PlaywrightScraper(
                    concurrency: options.Value.Punchup.Concurrency
                );
            });
        }
    }
}