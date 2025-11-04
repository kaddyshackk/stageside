using ComedyPull.Domain.Core.Shared;
using ComedyPull.Domain.Pipeline.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ComedyPull.Application.Pipeline.Collection
{
    public class CollectorFactory(IServiceProvider serviceProvider) : ICollectorFactory
    {
        public IDynamicCollector? GetPageCollector(Sku sku)
        {
            return serviceProvider.GetKeyedService<IDynamicCollector>(sku);
        }
    }
}