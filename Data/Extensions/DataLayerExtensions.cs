using ComedyPull.Application.Interfaces;
using ComedyPull.Application.Modules.DataProcessing.Interfaces;
using ComedyPull.Application.Modules.DataProcessing.Steps.Complete.Interfaces;
using ComedyPull.Application.Modules.DataProcessing.Steps.Transform.Interfaces;
using ComedyPull.Application.Modules.DataSync.Interfaces;
using ComedyPull.Application.Modules.Public.Events.GetEventBySlug.Interfaces;
using ComedyPull.Data.Queue;
using ComedyPull.Data.Modules.DataProcessing;
using ComedyPull.Data.Modules.DataProcessing.Complete;
using ComedyPull.Data.Modules.DataProcessing.Transform;
using ComedyPull.Data.Modules.DataSync;
using ComedyPull.Data.Modules.Public.Events.GetEventBySlug;
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
            services.AddSingleton<IQueue<BronzeRecord>, InMemoryQueue<BronzeRecord>>();

            services.AddPublicDataModule(configuration);
            services.AddDataProcessingDataModule(configuration);
            services.AddDataSyncDataModule(configuration);
        }

        private static void AddPublicDataModule(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<GetEventBySlugContext>(options =>
            {
                DbContextConfigurationUtil.ConfigureDbContextOptionsBuilder(options, configuration);
            });
            services.AddScoped<IGetEventBySlugRepository, GetEventBySlugRepository>();
        }

        private static void AddDataProcessingDataModule(this IServiceCollection services, IConfiguration configuration)
        {
            // Batch Context (shared across all processing states)
            services.AddDbContextFactory<BatchContext>((_, options) =>
            {
                DbContextConfigurationUtil.ConfigureDbContextOptionsBuilder(options, configuration);
            });
            services.AddSingleton<IBatchRepository, BatchRepository>();

            // Transform State
            services.AddDbContextFactory<TransformStateContext>((_, options) =>
            {
                DbContextConfigurationUtil.ConfigureDbContextOptionsBuilder(options, configuration);
            });
            services.AddSingleton<ITransformStateRepository, TransformStateRepository>();

            // Complete State
            services.AddDbContextFactory<CompleteStateContext>((_, options) =>
            {
                DbContextConfigurationUtil.ConfigureDbContextOptionsBuilder(options, configuration);
            });
            services.AddSingleton<ICompleteStateRepository, CompleteStateRepository>();
        }

        private static void AddDataSyncDataModule(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContextFactory<DataSyncContext>((_, options) =>
            {
                DbContextConfigurationUtil.ConfigureDbContextOptionsBuilder(options, configuration);
            });
            services.AddSingleton<IBronzeRecordIngestionRepository, BronzeRecordIngestionRepository>();
        }
    }
}