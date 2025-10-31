using ComedyPull.Data.Contexts.ComedyDb;
using ComedyPull.Data.Contexts.PipelineDb;
using ComedyPull.Data.Repositories;
using ComedyPull.Data.Services;
using ComedyPull.Data.Utils;
using ComedyPull.Domain.Interfaces.Processing;
using ComedyPull.Domain.Interfaces.Repository;
using ComedyPull.Domain.Interfaces.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;

namespace ComedyPull.Data.Extensions
{
    /// <summary>
    /// Defines methods for adding injectable services.
    /// </summary>
    public static class DataLayerExtensions
    {
        /// <summary>
        /// Configures services for the Data layer.
        /// </summary>
        /// <param name="services">Injected <see cref="IServiceCollection"/> instance.</param>
        /// <param name="configuration">Injected <see cref="IConfiguration"/> instance.</param>
        public static void AddDataLayer(this IServiceCollection services, IConfiguration configuration)
        {
            // Services
            services.AddSingleton<ISitemapLoader, SitemapLoader>();
            services.AddSingleton<IQueueClient, RedisQueueClient>();
            services.AddSingleton<IQueueHealthChecker, QueueHealthChecker>();
            
            // Web Browser Services
            services.AddSingleton<IPlaywright>(_ => Playwright.CreateAsync().Result);
            services.AddSingleton<IWebBrowser, PlaywrightWebBrowserAdapter>();
            
            // Repositories
            services.AddScoped<ISchedulingRepository, SchedulingRepository>();
            services.AddScoped<IActRepository, ActRepository>();
            services.AddScoped<IVenueRepository, VenueRepository>();
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IEventActRepository, EventActRepository>();
            
            // Contexts
            services.AddDbContextFactory<ComedyDbContext>((_, options) =>
            {
                DbContextConfigurationUtil.ConfigureDbContextOptionsBuilder(options, configuration);
            });
            
            services.AddDbContextFactory<PipelineDbContext>((_, options) =>
            {
                DbContextConfigurationUtil.ConfigureDbContextOptionsBuilder(options, configuration);
            });
        }
    }
}