namespace StageSide.Collection.WebBrowser;

public interface IWebPageSessionProvider
{
    public Task<IWebPageSession> CreateSessionAsync(CancellationToken ct);
}