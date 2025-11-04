using ComedyPull.Data.Contexts.ComedyDb;
using ComedyPull.Data.Contexts.PipelineDb;
using ComedyPull.Data.Core;
using ComedyPull.Data.Jobs;
using ComedyPull.Data.Services;
using ComedyPull.Data.Utils;
using ComedyPull.Domain.Core.Acts;
using ComedyPull.Domain.Core.Events.Interfaces;
using ComedyPull.Domain.Core.Venues.Interfaces;
using ComedyPull.Domain.Jobs.Interfaces;
using ComedyPull.Domain.Pipeline.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using StackExchange.Redis;

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
            services.AddScoped<IJobRepository, JobRepository>();
            services.AddScoped<IJobExecutionRepository, JobExecutionRepository>();
            services.AddScoped<IJobSitemapRepository, JobSitemapRepository>();
            services.AddScoped<IActRepository, ActRepository>();
            services.AddScoped<IVenueRepository, VenueRepository>();
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IEventActRepository, EventActRepository>();
            
            // Contexts
            services.AddDbContextFactory<ComedyDbContext>((_, options) =>
            {
                DbContextConfigurationUtil.ConfigureDbContextOptionsBuilder(options, configuration, "ComedyDb");
            });
            
            services.AddDbContextFactory<PipelineDbContext>((_, options) =>
            {
                DbContextConfigurationUtil.ConfigureDbContextOptionsBuilder(options, configuration, "PipelineDb");
            });
            
            // Redis
            services.AddSingleton<IConnectionMultiplexer>(_ =>
            {
                var connection = Environment.GetEnvironmentVariable("ConnectionStrings__Redis")
                                 ?? throw new Exception("Redis connection string not found");
                var config = ConfigurationOptions.Parse(connection);

                config.AbortOnConnectFail = false;
                config.ConnectRetry = 3;
                config.ConnectTimeout = 5000;
                
                return ConnectionMultiplexer.Connect(config);
            });
        }
    }
}