using ComedyPull.Api.Pipeline;
using ComedyPull.Api.Pipeline.Collection;
using ComedyPull.Api.Pipeline.Dispatching;
using ComedyPull.Api.Pipeline.Processing;
using ComedyPull.Api.Pipeline.Transformation;
using ComedyPull.Domain.Pipeline.Interfaces;
using ComedyPull.Domain.Queue;

namespace ComedyPull.Api.Extensions
{
    public static class ApiLayerExtensions
    {
        public static void AddApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<QueueOptions>(configuration.GetSection("Queue"));
            
            services.AddEndpointsApiExplorer();
            services.AddHttpClient();
            
            services.AddPipelineServices(configuration);
        }
        
        private static void AddPipelineServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Pipeline Hosted Services
            services.AddHostedService<DispatchingService>();
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
            services.Configure<DispatchingOptions>(configuration.GetSection("Pipeline:Dispatching"));
            services.Configure<DynamicCollectionOptions>(configuration.GetSection("Pipeline:DynamicCollection"));
            services.Configure<BackPressureOptions>(configuration.GetSection("Pipeline:BackPressure"));
        }
    }
}
