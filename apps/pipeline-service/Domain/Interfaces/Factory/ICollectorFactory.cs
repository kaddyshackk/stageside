using ComedyPull.Domain.Models;
using ComedyPull.Domain.Interfaces.Processing;

namespace ComedyPull.Domain.Interfaces.Factory
{
    public interface ICollectorFactory
    {
        public IDynamicCollector? GetPageCollector(ContentSku sku);
    }
}