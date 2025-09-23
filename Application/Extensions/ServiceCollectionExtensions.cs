using ComedyPull.Application.Features.DataSync.Interfaces;
using ComedyPull.Application.Features.DataSync.Punchup;
using ComedyPull.Application.Features.DataSync.Services;
using ComedyPull.Application.Options;
using ComedyPull.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;

namespace ComedyPull.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configures services for the Application layer.
        /// </summary>
        /// <param name="services">Injected <see cref="IServiceCollection"/> instance.</param>
        /// <param name="configuration">Injected <see cref="IConfiguration"/> instance.</param>
        public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDataSyncServices(configuration);
            services.AddDataProcessingServices(configuration);
            services.AddQuartzServices(configuration);
        }

        /// <summary>
        /// Configures services for the DataSync feature.
        /// </summary>
        /// <param name="services">Injected <see cref="IServiceCollection"/> instance.</param>
        /// <param name="configuration">Injected <see cref="IConfiguration"/> instance.</param>
        private static void AddDataSyncServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ScrapeOptions>(configuration.GetSection("ScrapeSettings"));
            services.AddScoped<ISitemapLoader, SitemapLoader>();

            // Processors
            services.AddTransient<PunchupTicketsPageCollector>();

            // Jobs
            services.AddScoped<PunchupScrapeJob>();

            // Scrapers
            services.AddKeyedSingleton<IScraper, PlaywrightScraper>(DataSourceKeys.Punchup,
                (provider, _) =>
                {
                    var options = provider.GetRequiredService<IOptions<ScrapeOptions>>();
                    return new PlaywrightScraper(
                        concurrency: options.Value.Punchup.Concurrency
                    );
                });
        }

        /// <summary>
        /// Configures services for the DataProcessing feature.
        /// </summary>
        /// <param name="services">Injected <see cref="IServiceCollection"/> instance.</param>
        /// <param name="configuration">Injected <see cref="IConfiguration"/> instance.</param>
        private static void AddDataProcessingServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Options
            services.Configure<DataProcessingOptions>(configuration.GetSection("DataProcessingOptions"));

            // Services
            services.AddHostedService<SourceRecordIngestionService>();
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