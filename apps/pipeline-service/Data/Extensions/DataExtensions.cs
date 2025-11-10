using ComedyPull.Data.Contexts.Comedy;
using ComedyPull.Data.Contexts.Scheduling;
using ComedyPull.Data.Services;
using ComedyPull.Data.Utils;
using ComedyPull.Domain.Pipeline.Interfaces;
using ComedyPull.Domain.Queue.Interfaces;
using ComedyPull.Domain.Scheduling.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using StackExchange.Redis;

namespace ComedyPull.Data.Extensions
{
    /// <summary>
    /// Defines methods for adding injectable services.
    /// </summary>
    public static class DataExtensions
    {
        /// <summary>
        /// Configures services for the Data layer.
        /// </summary>
        /// <param name="services">Injected <see cref="IServiceCollection"/> instance.</param>
        /// <param name="configuration">Injected <see cref="IConfiguration"/> instance.</param>
        public static void AddDataLayer(this IServiceCollection services, IConfiguration configuration)
        {
            // Contexts
            services.AddDbContextFactory<ComedyDbContext>((_, options) =>
            {
                DbContextConfigurationUtil.ConfigureDbContextOptionsBuilder(options, configuration, "ComedyDb");
            });
            
            services.AddDbContextFactory<SchedulingDbContext>((_, options) =>
            {
                DbContextConfigurationUtil.ConfigureDbContextOptionsBuilder(options, configuration, "SchedulingDb");
            });
            
            // Sessions
            services.AddScoped<IComedyDataSession, ComedyDataSession>();
            services.AddScoped<ISchedulingDataSession, SchedulingDataSession>();
            
            // Services
            services.AddSingleton<ISitemapLoader, SitemapLoader>();
            services.AddSingleton<IQueueClient, RedisQueueClient>();
            services.AddSingleton<IQueueHealthChecker, QueueHealthChecker>();
            
            // Web Browser Services
            services.AddSingleton<IPlaywright>(_ => Playwright.CreateAsync().Result);
            services.AddSingleton<IWebBrowser, PlaywrightWebBrowserAdapter>();
            
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