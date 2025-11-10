using ComedyPull.Domain.Pipeline.Interfaces;
using ComedyPull.Domain.Queue;
using ComedyPull.Service.Pipeline;
using ComedyPull.Service.Pipeline.Collection;
using ComedyPull.Service.Pipeline.Dispatching;
using ComedyPull.Service.Pipeline.Processing;
using ComedyPull.Service.Pipeline.Transformation;

namespace ComedyPull.Service.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddServiceLayer(this IServiceCollection services, IConfiguration configuration)
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
