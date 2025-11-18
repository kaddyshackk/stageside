using Microsoft.Extensions.DependencyInjection;
using StageSide.Domain.Models;
using StageSide.Pipeline.Interfaces;

namespace StageSide.SpaCollector.Domain.Collection;

public class PipelineAdapterFactory(IServiceProvider provider) : IPipelineAdapterFactory
{
    public IPipelineAdapter GetAdapter(SkuKey skuKey)
    {
        return provider.GetKeyedService<IPipelineAdapter>(skuKey)
            ?? throw new Exception($"Unable to find pipeline adapter for sku {skuKey}");
    }
}