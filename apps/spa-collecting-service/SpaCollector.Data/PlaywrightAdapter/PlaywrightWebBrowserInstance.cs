using Microsoft.Playwright;
using StageSide.Collection.WebBrowser;
using StageSide.Collection.WebBrowser.Params;

namespace StageSide.SpaCollector.Data.PlaywrightAdapter;

public class PlaywrightWebBrowserInstance(IBrowser browser) : IWebBrowserInstance
{
    public async Task<IWebBrowserContext> NewContextAsync(WebBrowserContextOptions? options = null)
    {
        var contextOptions = new BrowserNewContextOptions();
        if (options != null)
        {
            contextOptions.UserAgent = options.UserAgent;
            contextOptions.ViewportSize = new ViewportSize
            {
                Width = options.ViewportSize.Width,
                Height = options.ViewportSize.Height
            };
        }
        
        var context = await browser.NewContextAsync(contextOptions);
        return new PlaywrightWebBrowserContext(context);
    }

    public async Task CloseAsync()
    {
        await browser.CloseAsync();
    }
    
    public async ValueTask DisposeAsync()
    {
        await CloseAsync();
    }
}