namespace StageSide.Ticketmaster.Service;

public class TicketmasterWorker(IServiceScopeFactory scopeFactory, ILogger<TicketmasterWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        var provider = scope.ServiceProvider;
        var clientFactory = provider.GetRequiredService<IHttpClientFactory>();
        var client = clientFactory.CreateClient();

        try
        {
	        
	        


        }
        catch (Exception e)
        {
	        Console.WriteLine(e);
	        throw;
        }
        
    }
}
