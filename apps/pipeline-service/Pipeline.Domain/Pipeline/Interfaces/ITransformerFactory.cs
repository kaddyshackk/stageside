using StageSide.Pipeline.Domain.Models;

namespace StageSide.Pipeline.Domain.Pipeline.Interfaces
{
    public interface ITransformerFactory
    {
        public ITransformer? GetTransformer(Sku sku);
    }
}