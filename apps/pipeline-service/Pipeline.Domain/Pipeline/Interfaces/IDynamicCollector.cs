namespace StageSide.Pipeline.Domain.Pipeline.Interfaces
{
    public interface IDynamicCollector
    {
        public Task<object> CollectPageAsync(string url, IWebPage page);
    }
}