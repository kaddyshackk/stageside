using StageSide.Pipeline.Domain.Models;
using StageSide.Pipeline.Domain.PipelineAdapter;

namespace StageSide.Pipeline.Service.Pipeline;

public class PipelineAdapterFactory(IServiceProvider provider) : IPipelineAdapterFactory
{
    public IPipelineAdapter GetAdapter(Sku sku)
    {
        return provider.GetKeyedService<IPipelineAdapter>(sku)
            ?? throw new Exception($"Unable to find pipeline adapter for sku {sku}");
    }
}