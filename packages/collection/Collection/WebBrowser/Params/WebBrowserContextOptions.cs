namespace StageSide.Collection.WebBrowser.Params;

public class WebBrowserContextOptions
{
    public string? UserAgent { get; set; }
    public required WebViewportSize ViewportSize { get; set; }
}