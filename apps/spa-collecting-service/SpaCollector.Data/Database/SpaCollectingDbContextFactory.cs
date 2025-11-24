using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using StageSide.SpaCollector.Data.Utils;

namespace StageSide.SpaCollector.Data.Database;

public class SpaCollectingDbContextFactory : IDesignTimeDbContextFactory<SpaCollectingDbContext>
{
    public SpaCollectingDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        var optionsBuilder = new DbContextOptionsBuilder<SpaCollectingDbContext>();
        DbContextConfigurationUtil.ConfigureDbContextOptionsBuilder(optionsBuilder, configuration, "SchedulingDb");
        return new SpaCollectingDbContext(optionsBuilder.Options);
    }

    private static IConfiguration BuildConfiguration()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "SpaCollector.Service", "Settings"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables();
        return builder.Build();
    }
}