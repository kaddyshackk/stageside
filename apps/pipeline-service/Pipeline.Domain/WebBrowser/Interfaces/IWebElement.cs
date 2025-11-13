namespace StageSide.Pipeline.Domain.WebBrowser.Interfaces;

public interface IWebElement
{
    Task<string?> InnerTextAsync();
    Task<string?> GetAttributeAsync(string name);
    Task<bool> IsVisibleAsync();
    Task WaitForAsync();
    Task ClickAsync();
    Task<IReadOnlyList<IWebElement>> AllAsync();
    IWebElement Locator(string selector);
    IWebElement Nth(int index);
    IWebElement First { get; }
}