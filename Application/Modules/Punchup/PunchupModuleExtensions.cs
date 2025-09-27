using ComedyPull.Application.Modules.Punchup.Collectors;
using Microsoft.Extensions.DependencyInjection;

namespace ComedyPull.Application.Modules.Punchup;

public static class PunchupModuleExtensions
{
    public static void AddPunchupModule(this IServiceCollection services)
    {
        services.AddTransient<PunchupTicketsPageCollector>();
    }
}