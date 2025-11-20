using StageSide.Domain.Models;
using StageSide.Pipeline.Interfaces;
using StageSide.Punchup.Adapter;

namespace StageSide.Processor.Service.Extensions;

public static class SourceExtensions
{
    public static void AddSources(this IServiceCollection services, IConfiguration configuration)
    {
        AddPunchupSource(services);
    }

    private static void AddPunchupSource(this IServiceCollection services)
    {
        services.AddKeyedScoped<IPipelineAdapter, PunchupTicketsPageAdapter>(SkuKey.PunchupTicketsPage);
    }
}