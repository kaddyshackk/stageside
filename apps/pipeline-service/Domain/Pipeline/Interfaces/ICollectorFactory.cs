using ComedyPull.Domain.Models;

namespace ComedyPull.Domain.Pipeline.Interfaces
{
    public interface ICollectorFactory
    {
        public IDynamicCollector? GetPageCollector(Sku sku);
    }
}