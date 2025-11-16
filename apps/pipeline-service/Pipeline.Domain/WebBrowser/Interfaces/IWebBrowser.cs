namespace StageSide.Pipeline.Domain.WebBrowser.Interfaces;

public interface IWebBrowser : IDisposable
{
    IWebBrowserType Chromium { get; }
}
