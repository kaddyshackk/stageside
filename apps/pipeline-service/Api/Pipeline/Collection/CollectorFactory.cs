using ComedyPull.Domain.Models;
using ComedyPull.Domain.Pipeline.Interfaces;

namespace ComedyPull.Api.Pipeline.Collection
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