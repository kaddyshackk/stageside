namespace StageSide.Pipeline.Domain.WebBrowser.Options;

/// <summary>
/// Defines the configuration options for Playwright resource management.
/// </summary>
public class WebBrowserResourceOptions
{
    /// <summary>
    /// The number of browser instances to allow.
    /// </summary>
    public required int BrowserConcurrency { get; init; }
    
    /// <summary>
    /// The number of contexts to allow per browser.
    /// </summary>
    public required int ContextConcurrency { get; init; }
    
    /// <summary>
    /// The end of life handling strategy for browser contexts.
    /// </summary>
    public required WebBrowserContextStrategy ContextStrategy { get; init; }
}