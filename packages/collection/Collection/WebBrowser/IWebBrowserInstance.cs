using StageSide.Collection.WebBrowser.Params;

namespace StageSide.Collection.WebBrowser;

public interface IWebBrowserInstance : IAsyncDisposable
{
    public Task<IWebBrowserContext> NewContextAsync(WebBrowserContextOptions? options = null);
    public Task CloseAsync();
}