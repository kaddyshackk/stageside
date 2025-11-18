using Microsoft.Playwright;
using StageSide.Collection.WebBrowser.Interfaces;

namespace StageSide.SpaCollector.Data.PlaywrightAdapter;

public class PlaywrightWebBrowserContext(IBrowserContext context) : IWebBrowserContext
{
    public async Task<IWebPage> NewPageAsync()
    {
        var page = await context.NewPageAsync();
        return new PlaywrightWebPageAdapter(page);
    }

    public async Task CloseAsync()
    {
        await context.CloseAsync();
    }
    
    public async ValueTask DisposeAsync()
    {
        await CloseAsync();
    }
}