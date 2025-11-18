namespace StageSide.Collection.WebBrowser;

public interface IWebBrowser : IDisposable
{
    IWebBrowserType Chromium { get; }
}
