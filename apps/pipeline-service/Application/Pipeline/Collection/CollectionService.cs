using ComedyPull.Domain.Models;
using ComedyPull.Domain.Pipeline.Interfaces;
using ComedyPull.Domain.Pipeline.Models;
using ComedyPull.Domain.Queue.Interfaces;
using ComedyPull.Domain.Queue.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Context;

namespace ComedyPull.Application.Pipeline.Collection
{
    /// <summary>
    /// This service is responsible for pulling resource requests from the queue and delegating to collectors.
    /// </summary>
    public class CollectionService(
        IQueueClient queueClient,
        IBackPressureManager backPressureManager,
        IOptions<CollectionOptions> options,
        ILogger<CollectionService> logger)
        : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (LogContext.PushProperty("ServiceName", nameof(CollectionService)))
            {
                logger.LogInformation("Started {Service}", nameof(CollectionService));
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        if (await queueClient.GetLengthAsync(Queues.Collection) <= 0) continue;
                    
                        var context = await queueClient.DequeueAsync(Queues.Collection);
                        if (context == null) continue;

                        var targetQueue = GetTargetQueue(SkuConfiguration.GetCollectionType(context.Sku));

                        if (await backPressureManager.ShouldApplyBackPressureAsync(targetQueue))
                        {
                            await queueClient.EnqueueAsync(Queues.Collection, context);
                            continue;
                        }

                        await queueClient.EnqueueAsync(targetQueue, context);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error in collection service execution");
                    }
                    finally
                    {
                        await Task.Delay(TimeSpan.FromSeconds(options.Value.DelayIntervalSeconds), stoppingToken);
                    }
                }
                logger.LogInformation("Stopped {Service}", nameof(CollectionService));
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