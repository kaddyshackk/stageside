using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using StageSide.Collection.WebBrowser.Interfaces;
using StageSide.Collection.WebBrowser.Options;

namespace StageSide.Collection.WebBrowser;

public class WebBrowserResourceManager(IWebBrowser webBrowser, IOptions<WebBrowserResourceOptions> options) : IWebBrowserManager
{
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private readonly ConcurrentBag<WebBrowserContextPool> _pools = [];
    
    private bool _initialized;
    private bool _disposed;
    
    /// inheritdoc
    public async Task InitializeAsync(CancellationToken ct)
    {
        if (_initialized) return;
        
        await _initLock.WaitAsync(ct);
        try
        {
            if (_initialized) return;

            for (var i = 0; i < options.Value.BrowserConcurrency; i++)
            {
                _pools.Add(new WebBrowserContextPool
                {
                    Browser = await webBrowser.Chromium.LaunchAsync(),
                    Contexts = [],
                    Semaphore = new SemaphoreSlim(options.Value.ContextConcurrency)
                });
            }
            
            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }
    
    /// inheritdoc
    public async Task<IWebBrowserContext> AcquireContextAsync(CancellationToken ct)
    {
        if (!_initialized)
            throw new InvalidOperationException("Resource manager not initialized");
        
        var pool = _pools.OrderBy(p => p.Semaphore.CurrentCount).First();
        await pool.Semaphore.WaitAsync(ct);

        try
        {
            if (pool.Contexts.TryTake(out var existing))
            {
                if (IsContextValid(existing)) return existing;
                await existing.CloseAsync();
            }

            return await pool.Browser.NewContextAsync();
        }
        catch
        {
            pool.Semaphore.Release();
            throw;
        }
    }
    
    /// inheritdoc
    public async Task ReleaseContextAsync(IWebBrowserContext context, CancellationToken ct)
    {
        try
        {
            var pool = _pools.FirstOrDefault(p => p.Contexts.Contains(context));
            if (pool == null)
            {
                await context.CloseAsync();
                return;
            }

            switch (options.Value.ContextStrategy)
            {
                case WebBrowserContextStrategy.Dispose:
                    await context.CloseAsync();
                    break;
                case WebBrowserContextStrategy.Reuse:
                    // TODO: Clear cookies
                    pool.Contexts.Add(context);
                    break;
                default:
                    throw new Exception($"Unknown ContextReuseStrategy {options.Value.ContextStrategy}");
            }
            
            pool.Semaphore.Release();
        }
        catch
        {
            await context.CloseAsync();
        }
    }
    
    /// inheritdoc
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        foreach (var pool in _pools)
        {
            while (pool.Contexts.TryTake(out var context))
            {
                await context.CloseAsync();
            }

            await pool.Browser.CloseAsync();
            
            pool.Semaphore.Dispose();
        }

        webBrowser.Dispose();
        _initLock.Dispose();
        _disposed = true;
    }

    /// inheritdoc
    private static bool IsContextValid(IWebBrowserContext? context)
    {
        return context != null;
    }
}