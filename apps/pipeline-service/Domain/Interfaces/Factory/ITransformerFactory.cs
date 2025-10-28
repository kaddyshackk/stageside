using ComedyPull.Domain.Models;

namespace ComedyPull.Domain.Interfaces.Factory
{
    public interface ITransformerFactory
    {
        public ITransformer? GetTransformer(ContentSku sku);
    }
}