using ComedyPull.Domain.Core.Shared;

namespace ComedyPull.Domain.Pipeline.Interfaces
{
    public interface ITransformerFactory
    {
        public ITransformer? GetTransformer(Sku sku);
    }
}