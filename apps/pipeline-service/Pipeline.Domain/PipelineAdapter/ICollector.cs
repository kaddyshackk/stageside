namespace StageSide.Pipeline.Domain.PipelineAdapter
{
    public interface ICollector
    {
        public Task<object> CollectAsync(string url, CancellationToken ct);
    }
}