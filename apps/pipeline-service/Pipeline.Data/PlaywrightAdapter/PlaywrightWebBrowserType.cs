using Microsoft.Playwright;
using StageSide.Pipeline.Domain.WebBrowser.Interfaces;
using StageSide.Pipeline.Domain.WebBrowser.Options;

namespace StageSide.Pipeline.Data.PlaywrightAdapter;

public class PlaywrightWebBrowserType(IPlaywright playwright) : IWebBrowserType
{
    public async Task<IWebBrowserInstance> LaunchAsync(WebBrowserLaunchOptions? options = null)
    {
        var launchOptions = new BrowserTypeLaunchOptions();

        if (options != null)
        {
            launchOptions.Headless = options.Headless;
            launchOptions.Args = options.Args;
        }
        
        var browser = await playwright.Chromium.LaunchAsync(launchOptions);
        return new PlaywrightWebBrowserInstance(browser);
    }
}