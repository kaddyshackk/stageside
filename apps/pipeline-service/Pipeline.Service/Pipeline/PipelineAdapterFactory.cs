using StageSide.Domain.Models;
using StageSide.Pipeline.Domain.PipelineAdapter;

namespace StageSide.Pipeline.Service.Pipeline;

public class PipelineAdapterFactory(IServiceProvider provider) : IPipelineAdapterFactory
{
    public IPipelineAdapter GetAdapter(SkuKey skuKey)
    {
        return provider.GetKeyedService<IPipelineAdapter>(skuKey)
            ?? throw new Exception($"Unable to find pipeline adapter for sku {skuKey}");
    }
}