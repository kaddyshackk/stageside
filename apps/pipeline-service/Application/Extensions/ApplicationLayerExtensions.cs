using ComedyPull.Application.Pipeline;
using ComedyPull.Application.Pipeline.Collection;
using ComedyPull.Application.Pipeline.Processing;
using ComedyPull.Application.Pipeline.Scheduling;
using ComedyPull.Application.Pipeline.Transformation;
using ComedyPull.Domain.Interfaces.Factory;
using ComedyPull.Domain.Interfaces.Service;
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
            services.Configure<CollectionOptions>(configuration.GetSection("Pipeline:Collection"));
            services.Configure<TransformationOptions>(configuration.GetSection("Pipeline:Transformation"));
            services.Configure<ProcessingOptions>(configuration.GetSection("Pipeline:Processing"));
            services.Configure<SchedulingOptions>(configuration.GetSection("Pipeline:Scheduling"));
            services.Configure<DynamicCollectionOptions>(configuration.GetSection("Pipeline:DynamicCollection"));
            services.Configure<BackPressureOptions>(configuration.GetSection("Pipeline:BackPressure"));
            services.Configure<QueueOptions>(configuration.GetSection("Queue"));

            // Service Factories
            services.AddSingleton<ICollectorFactory, CollectorFactory>();
            services.AddSingleton<ITransformerFactory, TransformerFactory>();
            
            // Back Pressure Management
            services.AddSingleton<IBackPressureManager, BackPressureService>();

            // Pipeline services
            services.AddHostedService<SchedulingService>();
            services.AddHostedService<CollectionService>();
            services.AddHostedService<DynamicCollectionService>();
            services.AddHostedService<TransformationService>();
            services.AddHostedService<ProcessingService>();
        }
    }
}