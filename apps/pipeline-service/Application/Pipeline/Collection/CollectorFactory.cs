using ComedyPull.Domain.Interfaces.Factory;
using ComedyPull.Domain.Interfaces.Processing;
using ComedyPull.Domain.Models;
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