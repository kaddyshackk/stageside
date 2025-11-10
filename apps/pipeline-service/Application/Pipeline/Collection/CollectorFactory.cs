using ComedyPull.Domain.Models;
using ComedyPull.Domain.Pipeline.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ComedyPull.Application.Pipeline.Collection
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