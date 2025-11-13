using StageSide.Pipeline.Domain.WebBrowser.Options;

namespace StageSide.Pipeline.Domain.WebBrowser.Interfaces;

public interface IWebBrowserType
{
    Task<IWebBrowserInstance> LaunchAsync(WebBrowserLaunchOptions? options = null);
}