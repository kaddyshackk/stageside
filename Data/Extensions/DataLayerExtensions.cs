using ComedyPull.Application.Interfaces;
using ComedyPull.Application.Modules.DataProcessing.Interfaces;
using ComedyPull.Application.Modules.DataProcessing.Steps.Complete.Interfaces;
using ComedyPull.Application.Modules.DataProcessing.Steps.Transform.Interfaces;
using ComedyPull.Application.Modules.DataSync.Interfaces;
using ComedyPull.Application.Modules.Public.Events.GetEventBySlug.Interfaces;
using ComedyPull.Data.Modules.Common;
using ComedyPull.Data.Queue;
using ComedyPull.Data.Modules.DataProcessing;
using ComedyPull.Data.Modules.DataSync;
using ComedyPull.Data.Modules.Public;
using ComedyPull.Data.Utils;
using ComedyPull.Domain.Modules.DataProcessing;
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
        public static void AddDataServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure queue
            services.AddSingleton<IQueue<BronzeRecord>, InMemoryQueue<BronzeRecord>>();
            
            // Repositories
            services.AddSingleton<IBronzeRecordIngestionRepository, BronzeRecordIngestionRepository>();
            services.AddSingleton<IBatchRepository, BatchRepository>();
            services.AddSingleton<ICompleteStateRepository, CompleteStateRepository>();
            services.AddSingleton<ITransformStateRepository, TransformStateRepository>();
            services.AddScoped<IGetEventBySlugRepository, GetEventBySlugRepository>();

            // Context's
            services.AddDbContextFactory<ComedyPullContext>((_, options) =>
            {
                DbContextConfigurationUtil.ConfigureDbContextOptionsBuilder(options, configuration);
            });
        }
    }
}