namespace StageSide.Pipeline.Domain.WebBrowser.Options;

public class WebBrowserLaunchOptions
{
    public bool? Headless { get; set; }
    public string[] Args { get; set; } = null!;
}