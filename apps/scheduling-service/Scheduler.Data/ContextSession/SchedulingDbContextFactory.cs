using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using StageSide.Scheduler.Data.Utils;

namespace StageSide.Scheduler.Data.ContextSession
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
            DbContextConfigurationUtil.ConfigureDbContextOptionsBuilder(optionsBuilder, configuration, "SchedulingDb");
            return new SchedulingDbContext(optionsBuilder.Options);
        }

        private static IConfiguration BuildConfiguration()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "Scheduler.Service", "Settings"))
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables();
            return builder.Build();
        }
    }
}