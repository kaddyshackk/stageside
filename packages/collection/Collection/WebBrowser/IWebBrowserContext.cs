namespace StageSide.Collection.WebBrowser;

public interface IWebBrowserContext : IAsyncDisposable
{
    public Task<IWebPage> NewPageAsync();
    public Task CloseAsync();
}