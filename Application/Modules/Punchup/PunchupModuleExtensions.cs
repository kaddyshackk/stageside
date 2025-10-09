using ComedyPull.Application.Modules.DataProcessing.Steps.Interfaces;
using ComedyPull.Application.Modules.Punchup.Factories;
using ComedyPull.Application.Modules.Punchup.Processors.SubProcessors;
using ComedyPull.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace ComedyPull.Application.Modules.Punchup;

public static class PunchupModuleExtensions
{
    public static void AddPunchupModule(this IServiceCollection services)
    {
        services.AddScoped<IPunchupTicketsPageCollectorFactory, PunchupTicketsPageCollectorFactory>();

        // Register Punchup-specific sub-processors
        services.AddScoped<ISubProcessor<DataSource>, PunchupTransformSubProcessor>();
    }
}