using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StageSide.Scheduler.Data.Database;
using StageSide.Scheduler.Data.Utils;
using StageSide.Scheduler.Domain.Database;

namespace StageSide.Scheduler.Data.Extensions;

public static class DataExtensions
{
    public static void AddDataLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextFactory<SchedulingDbContext>((_, options) =>
        {
            DbContextConfigurationUtil.ConfigureDbContextOptionsBuilder(options, configuration, "SchedulingDb");
        });
        
        services.AddScoped<ISchedulingDbContextSession, SchedulingDbContextSession>();
    }
}