namespace StageSide.Scheduler.Service.Services;

public class SchedulingService(ILogger<SchedulingService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        logger.LogInformation("Started {ServiceName}", nameof(SchedulingService));
        while (!ct.IsCancellationRequested)
        {
            
        }
        logger.LogInformation("Stopped {ServiceName}", nameof(SchedulingService));
    }
}