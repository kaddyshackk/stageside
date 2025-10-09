using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ComedyPull.Data.Modules.DataSync
{
    /// <summary>
    /// Design-time factory for ProcessingContext to support EF migrations.
    /// </summary>
    public class DataSyncContextFactory : IDesignTimeDbContextFactory<DataSyncContext>
    {
        /// <summary>
        /// Creates a new instance of ProcessingContext for design-time operations.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>A new ProcessingContext instance.</returns>
        public DataSyncContext CreateDbContext(string[] args)
        {
            // Build configuration from the API project settings
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "Api", "Settings"))
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("DefaultConnection string is not configured.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<DataSyncContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new DataSyncContext(optionsBuilder.Options);
        }
    }
}