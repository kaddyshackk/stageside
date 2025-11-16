using Microsoft.Playwright;
using StageSide.Pipeline.Domain.WebBrowser;
using StageSide.Pipeline.Domain.WebBrowser.Interfaces;
using StageSide.Pipeline.Domain.WebBrowser.Options;

namespace StageSide.Pipeline.Data.PlaywrightAdapter;

public class PlaywrightWebPageAdapter(IPage page) : IWebPage
{
    public async Task GotoAsync(string url)
    {
        await page.GotoAsync(url);
    }

    public IWebElement Locator(string selector)
    {
        var locator = page.Locator(selector);
        return new PlaywrightWebElementAdapter(locator);
    }

    public IWebElement GetByRole(string role, WebElementParams? options = null)
    {
        var roleOptions = options != null ? new PageGetByRoleOptions { Name = options.Name } : null;
        var ariaRole = Enum.Parse<AriaRole>(role, true);
        var locator = page.GetByRole(ariaRole, roleOptions);
        return new PlaywrightWebElementAdapter(locator);
    }

    public IWebElement GetByText(string text)
    {
        var locator = page.GetByText(text);
        return new PlaywrightWebElementAdapter(locator);
    }

    public async Task CloseAsync()
    {
        await page.CloseAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await CloseAsync();
    }
}