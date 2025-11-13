using StageSide.Pipeline.Domain.Pipeline.Interfaces;
using StageSide.Pipeline.Domain.PipelineAdapter;
using StageSide.Pipeline.Domain.Queue;
using StageSide.Pipeline.Service.Pipeline;
using StageSide.Pipeline.Service.Pipeline.Collection;
using StageSide.Pipeline.Service.Pipeline.Dispatching;
using StageSide.Pipeline.Service.Pipeline.Processing;
using StageSide.Pipeline.Service.Pipeline.Transformation;

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
            services.AddHostedService<DynamicCollectionService>();
            services.AddHostedService<TransformationService>();
            services.AddHostedService<ProcessingService>();
            
            // Pipeline Management Services
            services.AddSingleton<IBackPressureManager, BackPressureService>();
            
            // Pipeline Helpers
            services.AddSingleton<IPipelineAdapterFactory, PipelineAdapterFactory>();
            
            // Options
            services.Configure<TransformationOptions>(configuration.GetSection("Pipeline:Transformation"));
            services.Configure<ProcessingOptions>(configuration.GetSection("Pipeline:Processing"));
            services.Configure<DispatchingOptions>(configuration.GetSection("Pipeline:Dispatching"));
            services.Configure<DynamicCollectionOptions>(configuration.GetSection("Pipeline:DynamicCollection"));
            services.Configure<BackPressureOptions>(configuration.GetSection("Pipeline:BackPressure"));
        }
    }
}
