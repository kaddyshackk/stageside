namespace ComedyPull.Application.Pipeline.Collection
{
    public class DynamicCollectionOptions
    {
        public int PollIntervalSeconds { get; init; }
        
        public int WebBrowserConcurrency { get; init; }
    }
}