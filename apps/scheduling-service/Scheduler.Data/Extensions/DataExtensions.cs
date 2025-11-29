using System.Reflection;
using DbUp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StageSide.Scheduler.Data.Database.SchedulingDb;
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

    public static void RunMigrations(this IServiceProvider provider)
    {
	    var configuration = provider.GetRequiredService<IConfiguration>();
	    var connection =  configuration.GetConnectionString("SchedulingDb");
	    
	    EnsureDatabase.For.PostgresqlDatabase(connection);

	    var upgrader = DeployChanges.To
		    .PostgresqlDatabase(connection)
		    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
		    .LogToConsole()
		    .Build();

	    var result = upgrader.PerformUpgrade();

	    if (!result.Successful)
	    {
		    throw new Exception($"Database migration failed: {result.Error}");
	    }
    }
}
