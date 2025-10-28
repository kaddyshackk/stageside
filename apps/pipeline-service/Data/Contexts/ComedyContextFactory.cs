using ComedyPull.Data.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ComedyPull.Data.Contexts
{
    /// <summary>
    /// Design-time factory for SchemaContext to support EF migrations.
    /// </summary>
    public class ComedyContextFactory : IDesignTimeDbContextFactory<ComedyContext>
    {
        public ComedyContext CreateDbContext(string[] args)
        {
            var configuration = BuildConfiguration();
            var optionsBuilder = new DbContextOptionsBuilder<ComedyContext>();
            DbContextConfigurationUtil.ConfigureDbContextOptionsBuilder(optionsBuilder, configuration);
            return new ComedyContext(optionsBuilder.Options);
        }

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