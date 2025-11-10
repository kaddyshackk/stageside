using ComedyPull.Data.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ComedyPull.Data.Contexts.Scheduling
{
    /// <summary>
    /// Design-time factory for PipelineDbContext to support EF migrations.
    /// </summary>
    public class SchedulingDbContextFactory : IDesignTimeDbContextFactory<SchedulingDbContext>
    {
        public SchedulingDbContext CreateDbContext(string[] args)
        {
            var configuration = BuildConfiguration();
            var optionsBuilder = new DbContextOptionsBuilder<SchedulingDbContext>();
            DbContextConfigurationUtil.ConfigureDbContextOptionsBuilder(optionsBuilder, configuration, "PipelineDb");
            return new SchedulingDbContext(optionsBuilder.Options);
        }

        private static IConfiguration BuildConfiguration()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "Api", "Settings"))
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddUserSecrets<SchedulingDbContextFactory>()
                .AddEnvironmentVariables();
            return builder.Build();
        }
    }
}