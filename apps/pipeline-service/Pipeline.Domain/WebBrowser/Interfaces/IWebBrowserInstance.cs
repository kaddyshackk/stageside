using StageSide.Pipeline.Domain.WebBrowser.Options;

namespace StageSide.Pipeline.Domain.WebBrowser.Interfaces;

public interface IWebBrowserInstance : IAsyncDisposable
{
    public Task<IWebBrowserContext> NewContextAsync(WebBrowserContextOptions? options = null);
    public Task CloseAsync();
}