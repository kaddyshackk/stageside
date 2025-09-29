using ComedyPull.Application.Modules.DataProcessing;
using ComedyPull.Application.Modules.Punchup;
using ComedyPull.Application.Modules.DataSync;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
    }
}