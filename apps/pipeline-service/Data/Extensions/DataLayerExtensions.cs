using ComedyPull.Data.Contexts;
using ComedyPull.Data.Services;
using ComedyPull.Data.Utils;
using ComedyPull.Domain.Interfaces.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            
            // Context's
            services.AddDbContextFactory<ComedyPullContext>((_, options) =>
            {
                DbContextConfigurationUtil.ConfigureDbContextOptionsBuilder(options, configuration);
            });
        }
    }
}