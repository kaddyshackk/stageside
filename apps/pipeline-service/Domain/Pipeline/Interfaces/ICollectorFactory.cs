using ComedyPull.Domain.Core.Shared;

namespace ComedyPull.Domain.Pipeline.Interfaces
{
    public interface ICollectorFactory
    {
        public IDynamicCollector? GetPageCollector(Sku sku);
    }
}