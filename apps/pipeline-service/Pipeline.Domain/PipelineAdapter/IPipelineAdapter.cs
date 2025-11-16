namespace StageSide.Pipeline.Domain.PipelineAdapter
{
    public interface IPipelineAdapter
    {
        public IScheduler GetScheduler();
        public ICollector GetCollector();
        public ITransformer GetTransformer();
    }
}