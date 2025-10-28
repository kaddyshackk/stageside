using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ComedyPull.Data.Utils
{
    public static class DbContextConfigurationUtil
    {
        public static void ConfigureDbContextOptionsBuilder(DbContextOptionsBuilder options, IConfiguration configuration)
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
                case "Development":
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                    break;
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