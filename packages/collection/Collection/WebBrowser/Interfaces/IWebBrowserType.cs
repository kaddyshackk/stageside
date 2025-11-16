using StageSide.Collection.WebBrowser.Options;

namespace StageSide.Collection.WebBrowser.Interfaces;

public interface IWebBrowserType
{
    Task<IWebBrowserInstance> LaunchAsync(WebBrowserLaunchOptions? options = null);
}