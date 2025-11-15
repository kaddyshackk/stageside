namespace StageSide.Pipeline.Domain.WebBrowser.Options;

public class WebBrowserContextOptions
{
    public string? UserAgent { get; set; }
    public required WebViewportSize ViewportSize { get; set; }
}