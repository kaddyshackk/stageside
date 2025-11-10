using ComedyPull.Domain.Models;

namespace ComedyPull.Domain.Pipeline.Interfaces
{
    public interface ITransformerFactory
    {
        public ITransformer? GetTransformer(Sku sku);
    }
}