using ComedyPull.Application.DataSync.Interfaces;
using ComedyPull.Application.DataSync.Punchup;
using ComedyPull.Application.DataSync.Services;
using ComedyPull.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ComedyPull.Application.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDataSyncServices(configuration);
        }
        
        private static void AddDataSyncServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ScrapeSettings>(configuration.GetSection("ScrapeSettings"));
            services.AddScoped<ISitemapLoader, SitemapLoader>();
            
            // Processors
            services.AddTransient<PunchupTicketsPageProcessor>();
            
            // Jobs
            services.AddScoped<PunchupScrapeJob>();
            
            // Scrapers
            services.AddKeyedSingleton<IScraper, PlaywrightScraper>(DataSource.Punchup, (provider, _) =>
            {
                var options = provider.GetRequiredService<IOptions<ScrapeSettings>>();
                return new PlaywrightScraper(
                    concurrency: options.Value.Punchup.Concurrency
                );
            });
        }
    }
}