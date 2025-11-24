using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StageSide.Processor.Data.Database.Comedy;
using StageSide.Processor.Data.Utils;
using StageSide.Processor.Domain.Database;

namespace StageSide.Processor.Data.Extensions;

public static class DataExtensions
{
    public static void AddDataLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextFactory<ComedyDbContext>((_, options) =>
        {
            DbContextConfigurationUtil.ConfigureDbContextOptionsBuilder(options, configuration, "ComedyDb");
        });
        services.AddScoped<IComedyDbContextSession, ComedyDbContextSession>();
    }
}