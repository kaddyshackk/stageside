using ComedyPull.Application.Modules.DataProcessing.Repositories.Interfaces;
using ComedyPull.Application.Modules.DataSync;
using ComedyPull.Data.Modules.DataSync.Contexts;
using ComedyPull.Data.Modules.DataSync.Repositories;
using ComedyPull.Data.Modules.DataProcessing.Contexts;
using ComedyPull.Data.Modules.DataProcessing.Repositories;
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
            services.AddDatabaseServices(configuration);
        }

        /// <summary>
        /// Configures database services.
        /// </summary>
        /// <param name="services">Injected <see cref="IServiceCollection"/> instance.</param>
        /// <param name="configuration">Injected <see cref="IConfiguration"/> instance.</param>
        /// <exception cref="InvalidOperationException">If the DefaultConnection or ASPNETCORE_EXCEPTION is misconfigured.</exception>
        private static void AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure Contexts
            
            // TODO: Move environment checks to Program.cs
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
            
            services.AddDbContextFactory<ComedyContext>((_, options) =>
            {
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
            });

            services.AddDbContextFactory<ProcessingContext>((_, options) =>
            {
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
            });
            
            // Configure Repositories

            services.AddSingleton<ISourceRecordWriteRepository, SourceRecordWriteRepository>();
            services.AddScoped<ISourceRecordRepository, SourceRecordRepository>();
            services.AddScoped<IComedyRepository, ComedyRepository>();
        }
    }
}