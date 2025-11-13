using StageSide.Pipeline.Domain.WebBrowser.Options;

namespace StageSide.Pipeline.Domain.WebBrowser.Interfaces;

public interface IWebPage : IAsyncDisposable
{
    Task GotoAsync(string url);
    IWebElement Locator(string selector);
    IWebElement GetByRole(string role, WebElementOptions? options = null);
    IWebElement GetByText(string text);
    Task CloseAsync();
}