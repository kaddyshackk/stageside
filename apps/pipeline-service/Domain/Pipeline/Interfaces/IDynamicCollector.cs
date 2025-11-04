namespace ComedyPull.Domain.Pipeline.Interfaces
{
    public interface IDynamicCollector
    {
        public Task<string> CollectPageAsync(string url, IWebPage page);
    }
}