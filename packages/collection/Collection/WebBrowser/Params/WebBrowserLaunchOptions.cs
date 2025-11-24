namespace StageSide.Collection.WebBrowser.Params;

public class WebBrowserLaunchOptions
{
    public bool Headless { get; init; } = true;
    public string[] Args { get; init; } = null!;
}