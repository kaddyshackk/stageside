using StageSide.Pipeline.Domain.Models;
using StageSide.Pipeline.Domain.Pipeline.Interfaces;

namespace StageSide.Pipeline.Service.Pipeline.Collection
{
    public class CollectorFactory(IServiceProvider serviceProvider) : ICollectorFactory
    {
        public IDynamicCollector? GetPageCollector(Sku sku)
        {
            using var scope = serviceProvider.CreateScope();
            return scope.ServiceProvider.GetKeyedService<IDynamicCollector>(sku);
        }
    }
}