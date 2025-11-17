using Microsoft.Extensions.Options;
using StageSide.Scheduler.Domain.Dispatching;

namespace StageSide.Scheduler.Service.Dispatching;

public class DispatchingService(ExecutionService service, IOptions<DispatchingServiceOptions> options, ILogger<DispatchingService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        logger.LogInformation("Started {ServiceName}", nameof(DispatchingService));
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var nextSchedule = await service.GetNextSchedule(ct);
                if (nextSchedule == null) continue;
                
                var job = await service.CreateJobAsync(nextSchedule.Id, ct);

                // TODO: Emit message that job is created and include schedule details
                
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in dispatching service execution");
            }
            finally
            {
                await Task.Delay(TimeSpan.FromSeconds(options.Value.DelayIntervalSeconds), ct);
            }
        }
        logger.LogInformation("Stopped {ServiceName}", nameof(DispatchingService));
    }
}