using MassTransit;
using Microsoft.Extensions.Options;
using StageSide.Contracts.Scheduling.Commands;
using StageSide.Domain.Models;
using StageSide.Scheduler.Domain.Dispatching;

namespace StageSide.Scheduler.Service.Dispatching;

public class DispatchingService(IServiceScopeFactory scopeFactory, IOptions<DispatchingServiceOptions> options, ILogger<DispatchingService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        logger.LogInformation("Started {ServiceName}", nameof(DispatchingService));
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var service = scope.ServiceProvider.GetRequiredService<ExecutionService>();
                var endpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
                
                var nextSchedule = await service.GetNextScheduleAsync(ct);
                if (nextSchedule == null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(options.Value.DelayIntervalSeconds), ct);
                    continue;
                }
                
                var job = await service.CreateJobAsync(nextSchedule.Id, ct);

                switch (nextSchedule.Sku.Type)
                {
                    case SkuType.Spa:
                    {
                        await endpoint.Publish(new StartSpaCollectionJobCommand
                        {
                            JobId = job.Id,
                            SkuId = nextSchedule.Sku.Id,
                            SkuName = nextSchedule.Sku.Name,
                        }, ct);
                        break;
                    }
                    case SkuType.Api:
                        throw new NotImplementedException($"Collection of api data sources is not supported.");
                    default:
                        throw new ArgumentException($"Sku of {nameof(nextSchedule.Sku.Type)} is invalid");
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