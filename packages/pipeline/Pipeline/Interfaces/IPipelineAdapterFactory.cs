using StageSide.Domain.Models;

namespace StageSide.Pipeline.Interfaces
{
    public interface IPipelineAdapterFactory
    {
        public IPipelineAdapter GetAdapter(Sku sku);
    }
}