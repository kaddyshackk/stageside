using StageSide.Pipeline.Domain.Models;
using StageSide.Pipeline.Domain.Pipeline.Interfaces;

namespace StageSide.Pipeline.Service.Pipeline.Transformation
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