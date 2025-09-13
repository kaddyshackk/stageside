using ComedyPull.Application.Enums;
using ComedyPull.Application.Features.DataProcessing.Interfaces;
using ComedyPull.Application.Interfaces;
using ComedyPull.Data.Database.Contexts;
using ComedyPull.Data.Database.Repositories;
using ComedyPull.Data.Queue;
using ComedyPull.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ComedyPull.Data.Extensions
{
    /// <summary>
    /// Defines methods for adding injectable services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configures services for the Data layer.
        /// </summary>
        /// <param name="services">Injected <see cref="IServiceCollection"/> instance.</param>
        /// <param name="configuration">Injected <see cref="IConfiguration"/> instance.</param>
        public static void AddDataServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDatabaseServices(configuration);
            services.AddQueueServices(configuration);
        }

        /// <summary>
        /// Configures database services.
        /// </summary>
        /// <param name="services">Injected <see cref="IServiceCollection"/> instance.</param>
        /// <param name="configuration">Injected <see cref="IConfiguration"/> instance.</param>
        /// <exception cref="InvalidOperationException">If the DefaultConnection or ASPNETCORE_EXCEPTION is misconfigured.</exception>
        private static void AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure DbContext's
            // TODO: Move database configuration to dedicated place
            
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
            
            // Configure Repositories
            
            services.AddScoped<IBronzeRecordRepository, BronzeRecordRepository>();
        }

        /// <summary>
        /// Configures queue services.
        /// </summary>
        /// <param name="services">Injected <see cref="IServiceCollection"/> instance.</param>
        /// <param name="configuration">Injected <see cref="IConfiguration"/> instance.</param>
        private static void AddQueueServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConnectionMultiplexer>(provider =>
            {
                var connectionString = configuration.GetConnectionString("Redis");
                if (string.IsNullOrEmpty(connectionString))
                    throw new Exception("ConnectionStrings:Redis is not configured.");
                return ConnectionMultiplexer.Connect(connectionString);
            });

            services.AddSingleton<IQueue<BronzeRecord>>(provider =>
            {
                var redis = provider.GetService<IConnectionMultiplexer>();
                if (redis is null)
                    throw new Exception("IConnectionMultiplexer is not configured.");
                var logger = provider.GetRequiredService<ILogger<RedisQueue<BronzeRecord>>>();
                return new RedisQueue<BronzeRecord>(redis, logger, QueueKey.BronzeRecord);
            });
        }
    }
}