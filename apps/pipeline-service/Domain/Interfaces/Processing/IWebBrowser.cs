namespace ComedyPull.Domain.Interfaces.Processing
{
    public interface IWebBrowser : IDisposable
    {
        IWebBrowserType Chromium { get; }
    }

    public interface IWebBrowserType
    {
        Task<IWebBrowserInstance> LaunchAsync(WebBrowserLaunchOptions? options = null);
    }

    public interface IWebBrowserInstance : IAsyncDisposable
    {
        Task<IWebBrowserContext> NewContextAsync(WebBrowserContextOptions? options = null);
        Task CloseAsync();
    }

    public interface IWebBrowserContext : IAsyncDisposable
    {
        Task<IWebPage> NewPageAsync();
        Task CloseAsync();
    }

    public class WebBrowserLaunchOptions
    {
        public bool? Headless { get; set; }
        public string[]? Args { get; set; }
    }

    public class WebBrowserContextOptions
    {
        public string? UserAgent { get; set; }
        public WebViewportSize? ViewportSize { get; set; }
    }

    public class WebViewportSize
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }
}