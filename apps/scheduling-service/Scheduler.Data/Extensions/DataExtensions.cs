using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StageSide.Scheduler.Data.ContextSession;
using StageSide.Scheduler.Data.Utils;
using StageSide.Scheduler.Domain.ContextSession;

namespace StageSide.Scheduler.Data.Extensions;

public static class DataExtensions
{
    public static void AddDataLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextFactory<SchedulingDbContext>((_, options) =>
        {
            DbContextConfigurationUtil.ConfigureDbContextOptionsBuilder(options, configuration, "SchedulingDb");
        });
        
        services.AddScoped<ISchedulingContextSession, SchedulingContextSession>();
    }
}