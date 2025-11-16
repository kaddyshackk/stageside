namespace StageSide.Collection.WebBrowser.Interfaces;

public interface IWebBrowser : IDisposable
{
    IWebBrowserType Chromium { get; }
}
