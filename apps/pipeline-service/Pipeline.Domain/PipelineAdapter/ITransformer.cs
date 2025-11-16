using StageSide.Pipeline.Domain.Pipeline.Models;

namespace StageSide.Pipeline.Domain.PipelineAdapter
{
    public interface ITransformer
    {
        public ICollection<ProcessedEntity> Transform(object data);
    }
}