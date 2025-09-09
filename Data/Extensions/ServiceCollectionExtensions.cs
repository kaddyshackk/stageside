using ComedyPull.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ComedyPull.Data.Extensions
{
    /// <summary>
    /// Defines methods for adding injectable services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configures services for the Data project.
        /// </summary>
        /// <param name="services">The injected service provider.</param>
        /// <param name="configuration">The injected configuration.</param>
        public static void AddDataServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDatabaseServices(configuration);
        }

        /// <summary>
        /// Configures database services.
        /// </summary>
        /// <param name="services">The injected service provider.</param>
        /// <param name="configuration">The injected configuration.</param>
        /// <exception cref="InvalidOperationException">If the DefaultConnection or ASPNETCORE_EXCEPTION is misconfigured.</exception>
        private static void AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ComedyContext>((serviceProvider, options) =>
            {
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
                
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                switch (environment)
                {
                    case "Development":
                        options.EnableSensitiveDataLogging();
                        options.EnableDetailedErrors();
                        options.LogTo(Console.WriteLine, LogLevel.Information);
                        break;
                    case "Production":
                        options.EnableServiceProviderCaching();
                        options.EnableSensitiveDataLogging(false);
                        break;
                    default:
                        throw new InvalidOperationException("Unknown environment: " + environment);
                }
            });

            services.AddScoped<ComedyContext>();
        }
    }
}