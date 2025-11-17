using StageSide.Pipeline.Models;

namespace StageSide.Pipeline.Interfaces
{
    public interface ITransformer
    {
        public ICollection<ProcessedEntity> Transform(object data);
    }
}