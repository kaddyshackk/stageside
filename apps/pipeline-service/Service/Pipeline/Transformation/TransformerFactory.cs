using ComedyPull.Domain.Models;
using ComedyPull.Domain.Pipeline.Interfaces;

namespace ComedyPull.Service.Pipeline.Transformation
{
    public class TransformerFactory(IServiceScopeFactory serviceFactory) : ITransformerFactory
    {
        public ITransformer? GetTransformer(Sku sku)
        {
            using var scope = serviceFactory.CreateScope();
            return scope.ServiceProvider.GetKeyedService<ITransformer>(sku);
        }
    }
}