using StageSide.Collection.WebBrowser.Params;

namespace StageSide.Collection.WebBrowser;

public interface IWebBrowserType
{
    Task<IWebBrowserInstance> LaunchAsync(WebBrowserLaunchOptions? options = null);
}