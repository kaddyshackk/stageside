namespace StageSide.Pipeline.Domain.WebBrowser.Interfaces;

public interface IWebPageSessionProvider
{
    public Task<IWebPageSession> CreateSessionAsync(CancellationToken ct);
}