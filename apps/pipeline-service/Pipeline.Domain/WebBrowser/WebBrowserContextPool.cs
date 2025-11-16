using System.Collections.Concurrent;
using StageSide.Pipeline.Domain.WebBrowser.Interfaces;

namespace StageSide.Pipeline.Domain.WebBrowser;

public class WebBrowserContextPool
{
    public IWebBrowserInstance Browser { get; set; } = null!;
    public ConcurrentBag<IWebBrowserContext> Contexts { get; set; } = null!;
    public SemaphoreSlim Semaphore { get; set; } = null!;
}