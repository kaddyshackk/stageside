using ComedyPull.Domain.Interfaces.Factory;
using ComedyPull.Domain.Interfaces.Processing;
using ComedyPull.Domain.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ComedyPull.Application.Services
{
    public class ServiceFactory(IServiceProvider serviceProvider) : ICollectorFactory, ITransformerFactory
    {
        public IDynamicCollector? GetPageCollector(Sku sku)
        {
            return serviceProvider.GetKeyedService<IDynamicCollector>(sku);
        }

        public ITransformer? GetTransformer(Sku sku)
        {
            return serviceProvider.GetKeyedService<ITransformer>(sku);
        }
    }
}