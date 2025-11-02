using ComedyPull.Application.Http;
using ComedyPull.Application.Http.Jobs.CreateJob;
using ComedyPull.Application.Pipeline;
using ComedyPull.Application.Pipeline.Collection;
using ComedyPull.Application.Pipeline.Processing;
using ComedyPull.Application.Pipeline.Scheduling;
using ComedyPull.Application.Pipeline.Transformation;
using ComedyPull.Domain.Interfaces.Factory;
using ComedyPull.Domain.Interfaces.Service;
using ComedyPull.Domain.Jobs.Operations;
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
            services.Configure<QueueOptions>(configuration.GetSection("Queue"));

            services.AddHttpServices();
            services.AddPipelineServices(configuration);
        }

        private static void AddHttpServices(this IServiceCollection services)
        {
            // Handlers
            services.AddScoped<IHandler<CreateJobCommand, CreateJobResponse>, CreateJobHandler>();
        }

        private static void AddPipelineServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Pipeline Hosted Services
            services.AddHostedService<SchedulingService>();
            services.AddHostedService<CollectionService>();
            services.AddHostedService<DynamicCollectionService>();
            services.AddHostedService<TransformationService>();
            services.AddHostedService<ProcessingService>();
            
            // Pipeline Management Services
            services.AddSingleton<IBackPressureManager, BackPressureService>();
            
            // Service Factories
            services.AddSingleton<ICollectorFactory, CollectorFactory>();
            services.AddSingleton<ITransformerFactory, TransformerFactory>();
            
            // Options
            services.Configure<CollectionOptions>(configuration.GetSection("Pipeline:Collection"));
            services.Configure<TransformationOptions>(configuration.GetSection("Pipeline:Transformation"));
            services.Configure<ProcessingOptions>(configuration.GetSection("Pipeline:Processing"));
            services.Configure<SchedulingOptions>(configuration.GetSection("Pipeline:Scheduling"));
            services.Configure<DynamicCollectionOptions>(configuration.GetSection("Pipeline:DynamicCollection"));
            services.Configure<BackPressureOptions>(configuration.GetSection("Pipeline:BackPressure"));
        }
    }
}