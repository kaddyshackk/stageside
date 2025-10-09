using ComedyPull.Application.Modules.DataProcessing.Steps.Complete.Interfaces;
using ComedyPull.Application.Modules.DataProcessing.Steps.Transform.Interfaces;
using ComedyPull.Application.Modules.DataSync;
using ComedyPull.Data.Modules.DataProcessing.Complete;
using ComedyPull.Data.Modules.DataProcessing.Transform;
using ComedyPull.Data.Modules.DataSync;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ComedyPull.Data.Extensions
{
    /// <summary>
    /// Defines methods for adding injectable services.
    /// </summary>
    public static class DataLayerExtensions
    {
        /// <summary>
        /// Configures services for the Data layer.
        /// </summary>
        /// <param name="services">Injected <see cref="IServiceCollection"/> instance.</param>
        /// <param name="configuration">Injected <see cref="IConfiguration"/> instance.</param>
        public static void AddDataServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDataProcessingServices(configuration);
            services.AddDataSyncServices(configuration);
        }

        private static void AddDataProcessingServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Transform State
            services.AddDbContextFactory<TransformStateContext>((_, options) =>
            {
                ConfigureDbContextOptionsBuilder(options, configuration);
            });
            services.AddSingleton<ITransformStateRepository, TransformStateRepository>();
            
            // Complete State
            services.AddDbContextFactory<CompleteStateContext>((_, options) =>
            {
                ConfigureDbContextOptionsBuilder(options, configuration);
            });
            services.AddSingleton<ICompleteStateRepository, CompleteStateRepository>();
        }

        private static void AddDataSyncServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContextFactory<DataSyncContext>((_, options) =>
            {
                ConfigureDbContextOptionsBuilder(options, configuration);
            });
            services.AddSingleton<IDataSyncRepository, DataSyncRepository>();
        }

        private static void ConfigureDbContextOptionsBuilder(DbContextOptionsBuilder options, IConfiguration configuration)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (string.IsNullOrEmpty(environment))
            {
                throw new InvalidOperationException("ASPNETCORE_ENVIRONMENT is not set");
            }
            
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("DefaultConnection string is not configured.");
            }
            
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
                    
                npgsqlOptions.CommandTimeout(30);
            });
            
            switch (environment)
            {
                case "Local":
                case "Development":
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                    options.LogTo(Console.WriteLine, LogLevel.Information);
                    break;
                case "Staging":
                case "Production":
                    options.EnableServiceProviderCaching();
                    options.EnableSensitiveDataLogging(false);
                    break;
                default:
                    throw new InvalidOperationException("Unknown environment: " + environment);
            }
        }
    }
}