using StageSide.Collection.WebBrowser.Interfaces;

namespace StageSide.Collection.WebBrowser;

public class WebPageSession : IWebPageSession
{
    public IWebPage Page { get; init; }
    
    public IWebBrowserContext Context { get; init; }
    
    private bool _disposed;
    
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        await Page.CloseAsync();
        _disposed = true;
    }
}