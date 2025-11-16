using StageSide.Pipeline.Domain.Pipeline.Interfaces;
using StageSide.Pipeline.Domain.PipelineAdapter;
using StageSide.Pipeline.Domain.Queue;
using StageSide.Pipeline.Service.Pipeline;
using StageSide.Pipeline.Service.Pipeline.Options;

namespace StageSide.Pipeline.Service.Extensions
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
            services.AddHostedService<TransformationService>();
            services.AddHostedService<ProcessingService>();
            
            // Pipeline Management Services
            services.AddSingleton<IBackPressureManager, BackPressureService>();
            
            // Pipeline Helpers
            services.AddSingleton<IPipelineAdapterFactory, PipelineAdapterFactory>();
            
            // Options
            services.Configure<BackPressureOptions>(configuration.GetSection("Pipeline:BackPressure"));
            services.Configure<DispatchingOptions>(configuration.GetSection("Pipeline:Dispatching"));
            services.Configure<CollectionOptions>(configuration.GetSection("Pipeline:Collection"));
            services.Configure<TransformationOptions>(configuration.GetSection("Pipeline:Transformation"));
            services.Configure<ProcessingOptions>(configuration.GetSection("Pipeline:Processing"));
        }
    }
}
