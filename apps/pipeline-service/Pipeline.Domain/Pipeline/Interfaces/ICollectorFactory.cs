using StageSide.Pipeline.Domain.Models;

namespace StageSide.Pipeline.Domain.Pipeline.Interfaces
{
    public interface ICollectorFactory
    {
        public IDynamicCollector? GetPageCollector(Sku sku);
    }
}