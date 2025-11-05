using ComedyPull.Domain.Core.Shared;
using ComedyPull.Domain.Pipeline.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ComedyPull.Application.Pipeline.Transformation
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