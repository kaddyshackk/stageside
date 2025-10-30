using ComedyPull.Domain.Interfaces.Factory;
using ComedyPull.Domain.Interfaces.Processing;
using ComedyPull.Domain.Models;
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