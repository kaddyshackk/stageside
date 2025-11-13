namespace StageSide.Pipeline.Domain.WebBrowser.Interfaces;

public interface IWebBrowserContext : IAsyncDisposable
{
    public Task<IWebPage> NewPageAsync();
    public Task CloseAsync();
}