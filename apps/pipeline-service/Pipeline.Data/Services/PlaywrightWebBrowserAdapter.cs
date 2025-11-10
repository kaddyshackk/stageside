using StageSide.Pipeline.Domain.Pipeline.Interfaces;
using Microsoft.Playwright;

namespace StageSide.Pipeline.Data.Services
{
    public class PlaywrightWebBrowserAdapter(IPlaywright playwright) : IWebBrowser
    {
        private bool _disposed;

        public IWebBrowserType Chromium => new PlaywrightWebBrowserType(playwright);

        public void Dispose()
        {
            if (_disposed) return;
            playwright?.Dispose();
            _disposed = true;
        }
    }

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

    public class PlaywrightWebBrowserInstance(IBrowser browser) : IWebBrowserInstance
    {
        public async Task<IWebBrowserContext> NewContextAsync(WebBrowserContextOptions? options = null)
        {
            var contextOptions = new BrowserNewContextOptions();
            
            if (options != null)
            {
                contextOptions.UserAgent = options.UserAgent;
                if (options.ViewportSize != null)
                {
                    contextOptions.ViewportSize = new ViewportSize 
                    { 
                        Width = options.ViewportSize.Width, 
                        Height = options.ViewportSize.Height 
                    };
                }
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

        public IWebElement GetByRole(string role, WebElementOptions? options = null)
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
}