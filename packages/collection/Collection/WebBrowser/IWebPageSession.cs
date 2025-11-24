namespace StageSide.Collection.WebBrowser;

public interface IWebPageSession : IAsyncDisposable
{
    public IWebPage Page { get; }
    public IWebBrowserContext Context { get; }
}