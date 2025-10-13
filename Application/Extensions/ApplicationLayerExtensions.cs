using ComedyPull.Application.Interfaces;
using ComedyPull.Application.Modules.DataProcessing.Events;
using ComedyPull.Application.Modules.DataProcessing.Services;
using ComedyPull.Application.Modules.DataProcessing.Services.Interfaces;
using ComedyPull.Application.Modules.DataProcessing.Steps.Complete;
using ComedyPull.Application.Modules.DataProcessing.Steps.Interfaces;
using ComedyPull.Application.Modules.DataProcessing.Steps.Transform;
using ComedyPull.Application.Modules.DataSync;
using ComedyPull.Application.Modules.DataSync.Interfaces;
using ComedyPull.Application.Modules.DataSync.Options;
using ComedyPull.Application.Modules.Punchup;
using ComedyPull.Application.Modules.Punchup.Collectors;
using ComedyPull.Application.Modules.Punchup.Collectors.Interfaces;
using ComedyPull.Application.Modules.Punchup.Processors;
using ComedyPull.Application.Modules.Queue;
using ComedyPull.Domain.Enums;
using ComedyPull.Domain.Modules.DataProcessing;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Quartz;

namespace ComedyPull.Application.Extensions
{
    public static class ApplicationLayerExtensions
    {
        /// <summary>
        /// Configures services for the Application layer.
        /// </summary>
        /// <param name="services">Injected <see cref="IServiceCollection"/> instance.</param>
        /// <param name="configuration">Injected <see cref="IConfiguration"/> instance.</param>
        public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddQuartzServices(configuration);

            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

            // Modules
            services.AddDataSyncModule(configuration);
            services.AddDataProcessingModule();
            services.AddPunchupModule();
            services.AddQueueModule();
        }
        
        /// <summary>
        /// Configures services for the DataSync module.
        /// </summary>
        /// <param name="services">Injected <see cref="IServiceCollection"/> instance.</param>
        /// <param name="configuration">Injected <see cref="IConfiguration"/> instance.</param>
        private static void AddDataSyncModule(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<DataSyncOptions>(configuration.GetSection("DataSyncOptions"));
            services.AddHostedService<BronzeRecordIngestionService>();
            services.AddScoped<ISitemapLoader, SitemapLoader>();
            services.AddScoped<IPlaywrightScraperFactory, PlaywrightScraperFactory>();
        }
        
        /// <summary>
        /// Configures services for the DataProcessing module.
        /// </summary>
        /// <param name="services">Injected <see cref="IServiceCollection"/> instance.</param>
        private static void AddDataProcessingModule(this IServiceCollection services)
        {
            services.AddScoped<INotificationHandler<StateCompletedEvent>, StateCompletedHandler>();
            services.AddScoped<ISubProcessorResolver, SubProcessorResolver>();

            // Register state processors
            services.AddScoped<IStateProcessor, TransformStateProcessor>();
            services.AddScoped<IStateProcessor, CompleteStateProcessor>();
        }

        private static void AddPunchupModule(this IServiceCollection services)
        {
            services.AddScoped<IPunchupTicketsPageCollectorFactory, PunchupTicketsPageCollectorFactory>();
            services.AddScoped<ISubProcessor<DataSourceType>, PunchupTransformSubProcessor>();
        }

        /// <summary>
        /// Configures queue services.
        /// </summary>
        /// <param name="services">Injected <see cref="IServiceCollection"/> instance.</param>
        private static void AddQueueModule(this IServiceCollection services)
        {
            services.AddSingleton<IQueue<BronzeRecord>>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<InMemoryQueue<BronzeRecord>>>();
                return new InMemoryQueue<BronzeRecord>(logger);
            });
        }
        
        /// <summary>
        /// Configures services for the Quartz scheduler.
        /// </summary>
        /// <param name="services">Injected <see cref="IServiceCollection"/> instance.</param>
        /// <param name="configuration">Injected <see cref="IConfiguration"/> instance.</param>
        private static void AddQuartzServices(this IServiceCollection services, IConfiguration configuration)
        {
            // TODO: Extract database specific logic to Data layer
            services.AddQuartzHostedService(options => 
            {
                options.WaitForJobsToComplete = true;
            });
            
            services.AddQuartz(q =>
            {
                // Configure Jobs
                
                q.AddJob<PunchupScrapeJob>(options =>
                {
                    options.WithIdentity(PunchupScrapeJob.Key);
                    options.WithDescription("Performs a complete scrape job for punchup.live.");
                    options.StoreDurably();
                });
                
                // Configure Postgres
                
                q.UsePersistentStore(options =>
                {
                    options.UsePostgres(connectionString =>
                    {
                        connectionString.ConnectionString = configuration.GetConnectionString("DefaultConnection")!;
                        connectionString.TablePrefix = "quartz.qrtz_";
                    });
                    options.UseNewtonsoftJsonSerializer();
                    options.PerformSchemaValidation = true;
                    options.UseProperties = true;
                });
                
                q.UseDefaultThreadPool(p =>
                {
                    p.MaxConcurrency = 10;
                });
            });
        }
    }
}