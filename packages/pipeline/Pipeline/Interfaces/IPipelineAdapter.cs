namespace StageSide.Pipeline.Interfaces
{
    public interface IPipelineAdapter
    {
        public IScheduler GetScheduler();
        public ICollector GetCollector();
        public ITransformer GetTransformer();
    }
}