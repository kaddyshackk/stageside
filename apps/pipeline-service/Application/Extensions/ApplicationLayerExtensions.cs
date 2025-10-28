using ComedyPull.Application.Options;
using ComedyPull.Application.Services;
using ComedyPull.Domain.Interfaces.Factory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ComedyPull.Application.Extensions
{
    public static class ApplicationLayerExtensions
    {
        /// <summary>
        /// Configures services for the Application layer.
        /// </summary>
        /// <param name="services">Injected <see cref="IServiceCollection"/> instance.</param>
        /// <param name="configuration">Injected <see cref="IConfiguration"/> instance.</param>
        public static void AddApplicationLayer(this IServiceCollection services, IConfiguration configuration)
        {
            // Options
            services.Configure<DataPipelineOptions>(configuration.GetSection("DataPipelineOptions"));
            
            // Service Factories
            services.AddSingleton<ServiceFactory>();
            services.AddSingleton<ICollectorFactory>(provider => provider.GetRequiredService<ServiceFactory>());
            services.AddSingleton<ITransformerFactory>(provider => provider.GetRequiredService<ServiceFactory>());

            // Pipeline services
            services.AddHostedService<SchedulingService>();
            services.AddHostedService<CollectionService>();
            services.AddHostedService<DynamicCollectionService>();
            services.AddHostedService<TransformationService>();
        }
    }
}