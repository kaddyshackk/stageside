using ComedyPull.Application.Modules.DataProcessing;
using ComedyPull.Application.Modules.Punchup;
using ComedyPull.Application.Modules.Punchup.Collectors;
using ComedyPull.Application.Modules.DataProcessing.Processors;
using ComedyPull.Application.Modules.DataSync.Configuration;
using ComedyPull.Application.Modules.DataSync.Services;
using ComedyPull.Application.Modules.DataSync.Services.Interfaces;
using ComedyPull.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
            services.AddDataSyncServices(configuration);
            services.AddDataProcessingServices();
            services.AddQuartzServices(configuration);
        }

        /// <summary>
        /// Configures services for the DataSync feature.
        /// </summary>
        /// <param name="services">Injected <see cref="IServiceCollection"/> instance.</param>
        /// <param name="configuration">Injected <see cref="IConfiguration"/> instance.</param>
        private static void AddDataSyncServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<DataSyncOptions>(configuration.GetSection("DataSyncOptions"));
            services.AddScoped<ISitemapLoader, SitemapLoader>();

            // Processors
            services.AddTransient<PunchupTicketsPageCollector>();

            // Scrapers
            services.AddKeyedSingleton<IScraper, PlaywrightScraper>(DataSourceKeys.Punchup,
                (provider, _) =>
                {
                    var options = provider.GetRequiredService<IOptions<DataSyncOptions>>();
                    return new PlaywrightScraper(
                        concurrency: options.Value.PunchupCollection.Concurrency
                    );
                });
            
            // Services
            services.AddHostedService<SourceRecordIngestionService>();
        }

        /// <summary>
        /// Configures services for the DataProcessing feature.
        /// </summary>
        /// <param name="services">Injected <see cref="IServiceCollection"/> instance.</param>
        private static void AddDataProcessingServices(this IServiceCollection services)
        {
            // Handlers
            services.AddScoped<INotificationHandler<StateCompletedEvent>, StateCompletedHandler>();
            
            // Processors
            services.AddScoped<TransformProcessor>();
        }
        
        /// <summary>
        /// Configures services for the Quartz scheduler.
        /// </summary>
        /// <param name="services">Injected <see cref="IServiceCollection"/> instance.</param>
        /// <param name="configuration">Injected <see cref="IConfiguration"/> instance.</param>
        private static void AddQuartzServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddQuartz(q =>
            {
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
                
                q.AddJob<PunchupScrapeJob>(options =>
                {
                    options.WithIdentity(PunchupScrapeJob.Key);
                    options.WithDescription("Performs a complete scrape job for punchup.live.");
                    options.StoreDurably();
                });
            });

            services.AddQuartzHostedService(options => 
            {
                options.WaitForJobsToComplete = true;
            });
        }
    }
}