using ComedyPull.Data.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ComedyPull.Data.Contexts.PipelineDb
{
    /// <summary>
    /// Design-time factory for PipelineDbContext to support EF migrations.
    /// </summary>
    public class PipelineDbContextFactory : IDesignTimeDbContextFactory<PipelineDbContext>
    {
        public PipelineDbContext CreateDbContext(string[] args)
        {
            var configuration = BuildConfiguration();
            var optionsBuilder = new DbContextOptionsBuilder<PipelineDbContext>();
            DbContextConfigurationUtil.ConfigureDbContextOptionsBuilder(optionsBuilder, configuration);
            return new PipelineDbContext(optionsBuilder.Options);
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