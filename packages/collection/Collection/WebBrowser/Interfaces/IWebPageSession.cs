namespace StageSide.Collection.WebBrowser.Interfaces;

public interface IWebPageSession : IAsyncDisposable
{
    public IWebPage Page { get; }
    public IWebBrowserContext Context { get; }
}