using StageSide.Collection.WebBrowser.Interfaces;

namespace StageSide.Collection.WebBrowser;

public class WebPageSessionProvider(IWebBrowserManager manager) : IWebPageSessionProvider
{
    public async Task<IWebPageSession> CreateSessionAsync(CancellationToken ct)
    {
        var context = await manager.AcquireContextAsync(ct);
        try
        {
            var page = await context.NewPageAsync();
            return new WebPageSession
            {
                Page = page,
                Context = context,
            };
        }
        catch
        {
            await manager.ReleaseContextAsync(context, ct);
            throw;
        }
    }
}