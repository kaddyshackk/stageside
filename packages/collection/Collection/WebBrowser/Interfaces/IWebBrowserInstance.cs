using StageSide.Collection.WebBrowser.Options;

namespace StageSide.Collection.WebBrowser.Interfaces;

public interface IWebBrowserInstance : IAsyncDisposable
{
    public Task<IWebBrowserContext> NewContextAsync(WebBrowserContextOptions? options = null);
    public Task CloseAsync();
}