using StageSide.Domain.Models;
using StageSide.Pipeline.Interfaces;
using StageSide.Punchup.Adapter;

namespace StageSide.SpaCollector.Service.Extensions;

public static class SourceExtensions
{
    public static void AddSources(this IServiceCollection services)
    {
        AddPunchupSource(services);
    }

    private static void AddPunchupSource(this IServiceCollection services)
    {
        services.AddScoped<PunchupTicketsPageCollector>();
        services.AddKeyedScoped<IPipelineAdapter, PunchupTicketsPageAdapter>(SkuKey.PunchupTicketsPage.GetEnumDescription());
    }
}