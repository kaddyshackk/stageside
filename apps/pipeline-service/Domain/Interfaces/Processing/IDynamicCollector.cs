namespace ComedyPull.Domain.Interfaces.Processing
{
    public interface IDynamicCollector
    {
        public Task<string> CollectPageAsync(string url, IWebPage page);
    }
}