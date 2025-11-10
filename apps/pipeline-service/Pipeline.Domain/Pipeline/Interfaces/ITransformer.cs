using StageSide.Pipeline.Domain.Pipeline.Models;

namespace StageSide.Pipeline.Domain.Pipeline.Interfaces
{
    public interface ITransformer
    {
        public ICollection<ProcessedEntity> Transform(object data);
    }
}