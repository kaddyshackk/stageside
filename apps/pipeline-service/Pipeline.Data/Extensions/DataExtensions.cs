using StageSide.Pipeline.Data.Contexts.Comedy;
using StageSide.Pipeline.Data.Services;
using StageSide.Pipeline.Data.Utils;
using StageSide.Pipeline.Domain.Pipeline.Interfaces;
using StageSide.Pipeline.Domain.Queue.Interfaces;
using StageSide.Pipeline.Domain.Scheduling.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using StackExchange.Redis;
using StageSide.Collection.WebBrowser.Interfaces;
using StageSide.Pipeline.Data.PlaywrightAdapter;

namespace StageSide.Pipeline.Data.Extensions
{
    /// <summary>
    /// Defines methods for adding injectable services.
    /// </summary>
    public static class DataExtensions
    {
        /// <summary>
        /// Configures services for the Pipeline.Data layer.
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
            
            // Sessions
            services.AddScoped<IComedyContextSession, ComedyContextSession>();
            
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