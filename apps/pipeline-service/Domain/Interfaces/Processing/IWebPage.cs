namespace ComedyPull.Domain.Interfaces.Processing
{
    public interface IWebPage : IAsyncDisposable
    {
        Task GotoAsync(string url);
        Task<IWebElement> QuerySelectorAsync(string selector);
        Task<IReadOnlyList<IWebElement>> QuerySelectorAllAsync(string selector);
        IWebElement Locator(string selector);
        IWebElement GetByRole(string role, WebElementOptions? options = null);
        IWebElement GetByText(string text);
        Task CloseAsync();
    }

    public interface IWebElement
    {
        Task<string?> InnerTextAsync();
        Task<string?> GetAttributeAsync(string name);
        Task<bool> IsVisibleAsync();
        Task WaitForAsync();
        Task ClickAsync();
        Task<IReadOnlyList<IWebElement>> QuerySelectorAllAsync(string selector);
        Task<IReadOnlyList<IWebElement>> AllAsync();
        IWebElement Locator(string selector);
        IWebElement Nth(int index);
        IWebElement First { get; }
    }

    public class WebElementOptions
    {
        public string? Name { get; set; }
    }
}