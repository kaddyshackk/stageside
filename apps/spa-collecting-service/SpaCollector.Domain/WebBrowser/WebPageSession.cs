using StageSide.Collection.WebBrowser;

namespace StageSide.SpaCollector.Domain.WebBrowser;

public class WebPageSession : IWebPageSession
{
    public IWebBrowserManager BrowserManager { get; init; }
    public IWebPage Page { get; init; }
    public IWebBrowserContext Context { get; init; }
    
    private bool _disposed;
    
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        await Page.CloseAsync();
        await BrowserManager.ReleaseContextAsync(Context, CancellationToken.None);
        _disposed = true;
    }
}