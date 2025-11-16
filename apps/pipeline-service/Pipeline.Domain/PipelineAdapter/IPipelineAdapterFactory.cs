using StageSide.Pipeline.Domain.Models;

namespace StageSide.Pipeline.Domain.PipelineAdapter
{
    public interface IPipelineAdapterFactory
    {
        public IPipelineAdapter GetAdapter(Sku sku);
    }
}