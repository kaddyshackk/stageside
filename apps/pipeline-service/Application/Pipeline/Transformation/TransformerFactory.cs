using ComedyPull.Domain.Core.Shared;
using ComedyPull.Domain.Pipeline.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ComedyPull.Application.Pipeline.Transformation
{
    public class TransformerFactory(IServiceProvider serviceProvider) : ITransformerFactory
    {
        public ITransformer? GetTransformer(Sku sku)
        {
            return serviceProvider.GetKeyedService<ITransformer>(sku);
        }
    }
}