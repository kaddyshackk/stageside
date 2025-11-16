using System.Collections.Concurrent;
using StageSide.Collection.WebBrowser.Interfaces;

namespace StageSide.Collection.WebBrowser;

public class WebBrowserContextPool
{
    public IWebBrowserInstance Browser { get; set; } = null!;
    public ConcurrentBag<IWebBrowserContext> Contexts { get; set; } = null!;
    public SemaphoreSlim Semaphore { get; set; } = null!;
}