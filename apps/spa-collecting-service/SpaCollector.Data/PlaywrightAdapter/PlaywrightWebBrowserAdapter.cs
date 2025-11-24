using Microsoft.Playwright;
using StageSide.Collection.WebBrowser;

namespace StageSide.SpaCollector.Data.PlaywrightAdapter;

public class PlaywrightWebBrowserAdapter(IPlaywright playwright) : IWebBrowser
{
    private bool _disposed;

    public IWebBrowserType Chromium => new PlaywrightWebBrowserType(playwright);

    public void Dispose()
    {
        if (_disposed) return;
        playwright.Dispose();
        _disposed = true;
    }
}