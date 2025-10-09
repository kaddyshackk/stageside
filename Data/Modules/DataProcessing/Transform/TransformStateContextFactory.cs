using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ComedyPull.Data.Modules.DataProcessing.Transform
{
    public class TransformStateContextFactory : IDesignTimeDbContextFactory<TransformStateContext>
    {
        public TransformStateContext CreateDbContext(string[] args)
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

            var optionsBuilder = new DbContextOptionsBuilder<TransformStateContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new TransformStateContext(optionsBuilder.Options);
        }
    }
}