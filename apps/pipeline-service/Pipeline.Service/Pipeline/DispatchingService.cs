using StageSide.Pipeline.Domain.Pipeline.Interfaces;
using StageSide.Pipeline.Domain.Queue.Interfaces;
using StageSide.Pipeline.Domain.Scheduling;
using Microsoft.Extensions.Options;
using Serilog.Context;
using StageSide.Pipeline.Domain.Models;
using StageSide.Pipeline.Domain.Pipeline.Models;
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
                        
                        var targetQueue = GetTargetQueue(SkuConfiguration.GetCollectionType(nextSchedule.Sku));
                        
                        if (await backPressureManager.ShouldApplyBackPressureAsync(targetQueue))
                        {
                            logger.LogWarning("{TargetQueue} queue is overloaded, delaying for another interval.", targetQueue);
                            continue;
                        }
                        
                        var job = await service.CreateJobAsync(nextSchedule.Id, ct);

                        var adapterFactory = scope.ServiceProvider.GetRequiredService<IPipelineAdapterFactory>();
                        var adapter = adapterFactory.GetAdapter(nextSchedule.Sku);

                        var entries = await adapter.GetScheduler().ScheduleAsync(nextSchedule, job, ct);
                        
                        await queueClient.EnqueueBatchAsync(targetQueue, entries);
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
        
        private static QueueConfig<PipelineContext> GetTargetQueue(CollectionType collectionType)
        {
            return collectionType switch
            {
                CollectionType.Dynamic => Queues.DynamicCollection,
                CollectionType.Static => throw new NotImplementedException("Static collector not implemented yet"),
                CollectionType.Api => throw new NotImplementedException("API collector not implemented yet"),
                _ => throw new NotSupportedException($"Collection type {collectionType} not supported")
            };
        }
    }
}