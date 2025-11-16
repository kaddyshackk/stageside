using StageSide.Pipeline.Domain.Pipeline.Interfaces;
using StageSide.Pipeline.Domain.Queue.Interfaces;
using StageSide.Pipeline.Domain.Scheduling;
using Microsoft.Extensions.Options;
using Serilog.Context;
using StageSide.Pipeline.Domain.PipelineAdapter;
using StageSide.Pipeline.Domain.Queue;
using StageSide.Pipeline.Service.Pipeline.Options;

namespace StageSide.Pipeline.Service.Pipeline
{
    public class DispatchingService(
        IServiceScopeFactory scopeFactory,
        IQueueClient queueClient,
        IBackPressureManager backPressureManager,
        IOptions<DispatchingOptions> options,
        ILogger<DispatchingService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            using (LogContext.PushProperty("ServiceName", nameof(DispatchingService)))
            {
                logger.LogInformation("Started {ServiceName}", nameof(DispatchingService));
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        using var scope = scopeFactory.CreateScope();
                        var service = scope.ServiceProvider.GetRequiredService<ExecutionService>();
                        
                        var nextSchedule = await service.GetNextSchedule(ct);
                        if (nextSchedule == null) continue;
                        
                        if (await backPressureManager.ShouldApplyBackPressureAsync(Queues.Collection))
                        {
                            logger.LogWarning("Collection queue is overloaded, delaying for another interval.");
                            continue;
                        }
                        
                        var job = await service.CreateJobAsync(nextSchedule.Id, ct);

                        var adapterFactory = scope.ServiceProvider.GetRequiredService<IPipelineAdapterFactory>();
                        var adapter = adapterFactory.GetAdapter(nextSchedule.Sku);

                        var entries = await adapter.GetScheduler().ScheduleAsync(nextSchedule, job, ct);
                        
                        await queueClient.EnqueueBatchAsync(Queues.Collection, entries);
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
    }
}