namespace StageSide.Pipeline.Domain.WebBrowser.Options;

public class WebBrowserLaunchOptions
{
    public bool Headless { get; init; } = true;
    public string[] Args { get; init; } = null!;
}