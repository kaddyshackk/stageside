using Microsoft.Playwright;
using StageSide.Collection.WebBrowser.Interfaces;

namespace StageSide.Pipeline.Data.PlaywrightAdapter;

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