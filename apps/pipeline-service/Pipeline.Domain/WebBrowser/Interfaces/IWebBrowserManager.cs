namespace StageSide.Pipeline.Domain.WebBrowser.Interfaces;

public interface IWebBrowserManager : IAsyncDisposable
{
    /// <summary>
    /// Initializes the manager and its resources. Starts the context pools.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    Task InitializeAsync(CancellationToken ct);
    
    /// <summary>
    /// Acquires a context from one of the context pools.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>Teh acquired context.</returns>
    Task<IWebBrowserContext> AcquireContextAsync(CancellationToken ct);
    
    /// <summary>
    /// Releases the provided context and returns it to the pool or disposes of it.
    /// </summary>
    /// <param name="context">The browser context to release.</param>
    /// <param name="ct">The cancellation token.</param>
    Task ReleaseContextAsync(IWebBrowserContext context, CancellationToken ct);
}