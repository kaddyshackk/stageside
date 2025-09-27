using ComedyPull.Application.Modules.Punchup.Factories;
using Microsoft.Extensions.DependencyInjection;

namespace ComedyPull.Application.Modules.Punchup;

public static class PunchupModuleExtensions
{
    public static void AddPunchupModule(this IServiceCollection services)
    {
        services.AddScoped<IPunchupTicketsPageCollectorFactory, PunchupTicketsPageCollectorFactory>();
    }
}