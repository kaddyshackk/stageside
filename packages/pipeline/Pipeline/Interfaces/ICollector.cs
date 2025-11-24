namespace StageSide.Pipeline.Interfaces
{
    public interface ICollector
    {
        public Task<object> CollectAsync(string url, CancellationToken ct);
    }
}