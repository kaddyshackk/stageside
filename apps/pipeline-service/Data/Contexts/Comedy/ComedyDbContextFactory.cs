using ComedyPull.Data.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ComedyPull.Data.Contexts.Comedy
{
    /// <summary>
    /// Design-time factory for ComedyDbContext to support EF migrations.
    /// </summary>
    public class ComedyDbContextFactory : IDesignTimeDbContextFactory<ComedyDbContext>
    {
        public ComedyDbContext CreateDbContext(string[] args)
        {
            var configuration = BuildConfiguration();
            var optionsBuilder = new DbContextOptionsBuilder<ComedyDbContext>();
            DbContextConfigurationUtil.ConfigureDbContextOptionsBuilder(optionsBuilder, configuration, "ComedyDb");
            return new ComedyDbContext(optionsBuilder.Options);
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