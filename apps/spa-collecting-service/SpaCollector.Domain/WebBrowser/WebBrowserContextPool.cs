using System.Collections.Concurrent;
using StageSide.Collection.WebBrowser;

namespace StageSide.SpaCollector.Domain.WebBrowser;

public class WebBrowserContextPool
{
    public IWebBrowserInstance Browser { get; set; } = null!;
    public ConcurrentBag<IWebBrowserContext> Contexts { get; set; } = null!;
    public SemaphoreSlim Semaphore { get; set; } = null!;
}