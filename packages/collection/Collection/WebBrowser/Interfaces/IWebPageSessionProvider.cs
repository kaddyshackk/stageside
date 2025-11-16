namespace StageSide.Collection.WebBrowser.Interfaces;

public interface IWebPageSessionProvider
{
    public Task<IWebPageSession> CreateSessionAsync(CancellationToken ct);
}