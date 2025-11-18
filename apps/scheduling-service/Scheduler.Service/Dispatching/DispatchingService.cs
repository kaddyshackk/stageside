using Microsoft.Extensions.Options;
using StageSide.Domain.Models;
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
                var nextSchedule = await service.GetNextScheduleAsync(ct);
                if (nextSchedule == null) continue;
                
                var job = await service.CreateJobAsync(nextSchedule.Id, ct);

                if (nextSchedule.Sku.CollectionType == CollectionType.Spa)
                {
                    // TODO: Emit command event to start collecting
                }
                else
                {
                    throw new ArgumentException($"{nameof(nextSchedule.Sku.CollectionConfigId)} is invalid");
                }
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