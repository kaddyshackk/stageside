using ComedyPull.Data.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ComedyPull.Data.Modules.Common
{
    /// <summary>
    /// Base factory for design-time DbContext creation that provides consistent configuration.
    /// </summary>
    /// <typeparam name="T">The DbContext type to create.</typeparam>
    public abstract class BaseDesignTimeDbContextFactory<T> : IDesignTimeDbContextFactory<T> 
        where T : DbContext
    {
        /// <summary>
        /// Creates a new instance of the DbContext for design-time operations.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>A new DbContext instance.</returns>
        public T CreateDbContext(string[] args)
        {
            var configuration = BuildConfiguration();
            var optionsBuilder = new DbContextOptionsBuilder<T>();
            DbContextConfigurationUtil.ConfigureDbContextOptionsBuilder(optionsBuilder, configuration);
            return CreateContext(optionsBuilder.Options);
        }

        /// <summary>
        /// Creates the specific DbContext instance with the provided options.
        /// </summary>
        /// <param name="options">The configured DbContext options.</param>
        /// <returns>A new DbContext instance.</returns>
        protected abstract T CreateContext(DbContextOptions<T> options);

        /// <summary>
        /// Builds the configuration from the API project settings.
        /// </summary>
        /// <returns>The configuration instance.</returns>
        private static IConfiguration BuildConfiguration()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "Api", "Settings"))
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables();

            return builder.Build();
        }
    }
}