using Microsoft.Playwright;
using StageSide.Collection.WebBrowser.Interfaces;

namespace StageSide.Pipeline.Data.PlaywrightAdapter;

public class PlaywrightWebElementAdapter(ILocator locator) : IWebElement
{
    public async Task<string?> InnerTextAsync()
    {
        return await locator.InnerTextAsync();
    }

    public async Task<string?> GetAttributeAsync(string name)
    {
        return await locator.GetAttributeAsync(name);
    }

    public async Task<bool> IsVisibleAsync()
    {
        return await locator.IsVisibleAsync();
    }

    public async Task WaitForAsync()
    {
        await locator.WaitForAsync();
    }

    public async Task ClickAsync()
    {
        await locator.ClickAsync();
    }

    public async Task<IReadOnlyList<IWebElement>> AllAsync()
    {
        var locators = await locator.AllAsync();
        return locators.Select(l => new PlaywrightWebElementAdapter(l)).ToList();
    }

    public IWebElement Locator(string selector)
    {
        var childLocator = locator.Locator(selector);
        return new PlaywrightWebElementAdapter(childLocator);
    }

    public IWebElement Nth(int index)
    {
        var nthLocator = locator.Nth(index);
        return new PlaywrightWebElementAdapter(nthLocator);
    }

    public IWebElement First => new PlaywrightWebElementAdapter(locator.First);
}