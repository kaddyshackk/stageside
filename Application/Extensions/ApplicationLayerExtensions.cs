using ComedyPull.Application.Interfaces;
using ComedyPull.Application.Modules.DataProcessing;
using ComedyPull.Application.Modules.Punchup;
using ComedyPull.Application.Modules.DataSync;
using ComedyPull.Application.Modules.Queue;
using ComedyPull.Domain.Models.Processing;
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
            services.AddPunchupModule();
            services.AddDataSyncModule(configuration);
            services.AddDataProcessingModule();
            services.AddQueueModule();
        }
        
        /// <summary>
        /// Configures services for the Quartz scheduler.
        /// </summary>
        /// <param name="services">Injected <see cref="IServiceCollection"/> instance.</param>
        /// <param name="configuration">Injected <see cref="IConfiguration"/> instance.</param>
        private static void AddQuartzServices(this IServiceCollection services, IConfiguration configuration)
        {
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

        /// <summary>
        /// Configures queue services.
        /// </summary>
        /// <param name="services">Injected <see cref="IServiceCollection"/> instance.</param>
        private static void AddQueueModule(this IServiceCollection services)
        {
            services.AddSingleton<IQueue<SourceRecord>>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<InMemoryQueue<SourceRecord>>>();
                return new InMemoryQueue<SourceRecord>(logger);
            });
        }
    }
}